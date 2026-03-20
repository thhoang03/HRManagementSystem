using HRManagementSystem.DAL;
using HRManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HRManagementSystem.BLL
{
    public class PayrollBLL : BaseBLL<Payroll>
    {
        private readonly PayrollDAL _payDAL;

        public PayrollBLL() : base(new PayrollDAL())
        {
            _payDAL = (PayrollDAL)_baseDAL;
        }

        public List<Payroll> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return _payDAL.GetAll().ToList();
            }

            return _payDAL.GetAll()
                .Where(p =>
                    (p.Employee != null && p.Employee.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    || (!string.IsNullOrEmpty(p.Status) && p.Status.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    || (p.Contract != null && !string.IsNullOrEmpty(p.Contract.ContractType)
                        && p.Contract.ContractType.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        public bool HasPayrolls(int month, int year)
        {
            using (var context = new HrmanagementSystemContext())
            {
                return context.Payrolls.Any(p => p.Month == month && p.Year == year);
            }
        }

        public List<PayrollPreview> GeneratePreview(int month, int year, int standardWorkDays = 26)
        {
            using (var context = new HrmanagementSystemContext())
            {
                var monthStart = new DateTime(year, month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var employees = context.Employees.AsNoTracking().ToList();
                var contracts = context.Contracts.AsNoTracking().ToList();
                var attendances = context.Attendances.AsNoTracking()
                    .Where(a => a.AttendanceDate.HasValue
                                && a.AttendanceDate.Value.Month == month
                                && a.AttendanceDate.Value.Year == year)
                    .ToList();

                var leaveRequests = context.LeaveRequests.AsNoTracking()
                    .Where(l => l.EmployeeId.HasValue
                                && l.Status != null
                                && l.Status.ToLower() == "approved"
                                && l.StartDate <= monthEnd
                                && l.EndDate >= monthStart)
                    .ToList();

                var contractByEmployee = contracts
                    .Where(c => c.EmployeeId.HasValue)
                    .GroupBy(c => c.EmployeeId!.Value)
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(c => c.ContractId).First());

                var workDaysByEmployee = attendances
                    .Where(a => !IsAbsentOrLeave(a.Status))
                    .GroupBy(a => a.EmployeeId ?? 0)
                    .ToDictionary(g => g.Key, g => g.Count());

                var leaveDaysByEmployee = leaveRequests
                    .Where(l => IsUnpaidLeave(l.LeaveType))
                    .GroupBy(l => l.EmployeeId!.Value)
                    .ToDictionary(g => g.Key, g => g.Sum(l => CountOverlapDays(l.StartDate, l.EndDate, monthStart, monthEnd)));

                var previews = new List<PayrollPreview>();
                foreach (var emp in employees)
                {
                    contractByEmployee.TryGetValue(emp.EmployeeId, out var contract);

                    decimal baseSalary = contract?.ContractSalary ?? 0m;
                    int workDays = workDaysByEmployee.TryGetValue(emp.EmployeeId, out var wd) ? wd : 0;
                    int leaveDays = leaveDaysByEmployee.TryGetValue(emp.EmployeeId, out var ld) ? ld : 0;

                    decimal salaryPerDay = standardWorkDays > 0 ? baseSalary / standardWorkDays : 0m;
                    decimal deduction = salaryPerDay * leaveDays;

                    var preview = new PayrollPreview
                    {
                        EmployeeId = emp.EmployeeId,
                        EmployeeName = emp.FullName ?? string.Empty,
                        ContractId = contract?.ContractId,
                        ContractType = contract?.ContractType,
                        Month = month,
                        Year = year,
                        BaseSalary = baseSalary,
                        WorkDays = workDays,
                        LeaveDays = leaveDays,
                        OvertimePay = 0m,
                        Bonus = 0m,
                        Deduction = deduction,
                        NetSalary = baseSalary - deduction,
                        Status = "Draft",
                        CreatedAt = DateTime.Now
                    };

                    previews.Add(preview);
                }

                return previews;
            }
        }

        public List<PayrollPreview> GetPayrollPreviews(int month, int year)
        {
            using (var context = new HrmanagementSystemContext())
            {
                var monthStart = new DateTime(year, month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var payrolls = context.Payrolls
                    .Include(p => p.Employee)
                    .Include(p => p.Contract)
                    .AsNoTracking()
                    .Where(p => p.Month == month && p.Year == year)
                    .ToList();

                var attendances = context.Attendances.AsNoTracking()
                    .Where(a => a.AttendanceDate.HasValue
                                && a.AttendanceDate.Value.Month == month
                                && a.AttendanceDate.Value.Year == year)
                    .ToList();

                var leaveRequests = context.LeaveRequests.AsNoTracking()
                    .Where(l => l.EmployeeId.HasValue
                                && l.Status != null
                                && l.Status.ToLower() == "approved"
                                && l.StartDate <= monthEnd
                                && l.EndDate >= monthStart)
                    .ToList();

                var workDaysByEmployee = attendances
                    .Where(a => !IsAbsentOrLeave(a.Status))
                    .GroupBy(a => a.EmployeeId ?? 0)
                    .ToDictionary(g => g.Key, g => g.Count());

                var leaveDaysByEmployee = leaveRequests
                    .Where(l => IsUnpaidLeave(l.LeaveType))
                    .GroupBy(l => l.EmployeeId!.Value)
                    .ToDictionary(g => g.Key, g => g.Sum(l => CountOverlapDays(l.StartDate, l.EndDate, monthStart, monthEnd)));

                return payrolls.Select(p =>
                {
                    int empId = p.EmployeeId ?? 0;
                    int workDays = workDaysByEmployee.TryGetValue(empId, out var wd) ? wd : 0;
                    int leaveDays = leaveDaysByEmployee.TryGetValue(empId, out var ld) ? ld : 0;

                    return new PayrollPreview
                    {
                        PayrollId = p.PayrollId,
                        EmployeeId = empId,
                        EmployeeName = p.Employee?.FullName ?? string.Empty,
                        ContractId = p.ContractId,
                        ContractType = p.Contract?.ContractType,
                        Month = p.Month ?? month,
                        Year = p.Year ?? year,
                        BaseSalary = p.BaseSalary ?? 0m,
                        WorkDays = workDays,
                        LeaveDays = leaveDays,
                        OvertimePay = p.OvertimePay ?? 0m,
                        Bonus = p.Bonus ?? 0m,
                        Deduction = p.Deduction ?? 0m,
                        NetSalary = p.NetSalary ?? 0m,
                        Status = p.Status ?? string.Empty,
                        CreatedAt = p.CreatedAt
                    };
                }).ToList();
            }
        }

        public void SavePayrolls(IEnumerable<PayrollPreview> previews)
        {
            using (var context = new HrmanagementSystemContext())
            {
                foreach (var preview in previews)
                {
                    var payroll = new Payroll
                    {
                        EmployeeId = preview.EmployeeId,
                        ContractId = preview.ContractId,
                        Month = preview.Month,
                        Year = preview.Year,
                        BaseSalary = preview.BaseSalary,
                        OvertimePay = preview.OvertimePay,
                        Deduction = preview.Deduction,
                        Bonus = preview.Bonus,
                        NetSalary = preview.NetSalary,
                        Status = "Draft",
                        CreatedAt = preview.CreatedAt ?? DateTime.Now
                    };

                    context.Payrolls.Add(payroll);
                }

                context.SaveChanges();
            }
        }

        public int UpdateStatus(int month, int year, string status)
        {
            using (var context = new HrmanagementSystemContext())
            {
                var payrolls = context.Payrolls.Where(p => p.Month == month && p.Year == year).ToList();
                foreach (var payroll in payrolls)
                {
                    payroll.Status = status;
                }

                return context.SaveChanges();
            }
        }

        private static bool IsAbsentOrLeave(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return false;
            }

            return status.Equals("Absent", StringComparison.OrdinalIgnoreCase)
                || status.Equals("Leave", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsUnpaidLeave(string? leaveType)
        {
            if (string.IsNullOrWhiteSpace(leaveType))
            {
                return false;
            }

            var normalized = leaveType.Trim().ToLowerInvariant();
            return normalized.Contains("without pay")
                || normalized.Contains("unpaid")
                || normalized.Contains("no pay");
        }

        private static int CountOverlapDays(DateTime start, DateTime end, DateTime monthStart, DateTime monthEnd)
        {
            var rangeStart = start.Date > monthStart ? start.Date : monthStart;
            var rangeEnd = end.Date < monthEnd ? end.Date : monthEnd;
            if (rangeEnd < rangeStart)
            {
                return 0;
            }

            return (rangeEnd - rangeStart).Days + 1;
        }
    }
}
