using HRManagementSystem.DAL;
using HRManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        public bool HasPayroll(int month, int year, int employeeId)
        {
            using (var context = new HrmanagementSystemContext())
            {
                return context.Payrolls.Any(p =>
                    p.Month == month
                    && p.Year == year
                    && p.EmployeeId == employeeId);
            }
        }

        public List<PayrollPreview> GeneratePreview(int month, int year, int standardWorkDays = 26)
        {
            using (var context = new HrmanagementSystemContext())
            {
                var monthStart = new DateTime(year, month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                var payrollSettings = LoadPayrollSettings(context, standardWorkDays);

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

                var lateMinutesByEmployee = attendances
                    .Where(a => a.EmployeeId.HasValue
                                && a.CheckIn.HasValue
                                && !IsAbsentOrLeave(a.Status))
                    .GroupBy(a => a.EmployeeId!.Value)
                    .ToDictionary(g => g.Key, g => g.Sum(a =>
                        CountLateMinutes(a.CheckIn!.Value, payrollSettings.WorkStartTime, payrollSettings.LateGraceMinutes)));

                var overtimeMinutesByEmployee = attendances
                    .Where(a => a.EmployeeId.HasValue
                                && a.CheckOut.HasValue
                                && !IsAbsentOrLeave(a.Status))
                    .GroupBy(a => a.EmployeeId!.Value)
                    .ToDictionary(g => g.Key, g => g.Sum(a =>
                        CountOvertimeMinutes(a.CheckOut!.Value, payrollSettings.WorkEndTime)));

                var previews = new List<PayrollPreview>();
                foreach (var emp in employees)
                {
                    contractByEmployee.TryGetValue(emp.EmployeeId, out var contract);

                    decimal baseSalary = contract?.ContractSalary ?? 0m;
                    int workDays = workDaysByEmployee.TryGetValue(emp.EmployeeId, out var wd) ? wd : 0;
                    int leaveDays = leaveDaysByEmployee.TryGetValue(emp.EmployeeId, out var ld) ? ld : 0;
                    int lateMinutes = lateMinutesByEmployee.TryGetValue(emp.EmployeeId, out var lm) ? lm : 0;
                    int overtimeMinutes = overtimeMinutesByEmployee.TryGetValue(emp.EmployeeId, out var otm) ? otm : 0;

                    decimal salaryPerDay = payrollSettings.StandardWorkDays > 0 ? baseSalary / payrollSettings.StandardWorkDays : 0m;
                    decimal salaryPerMinute = payrollSettings.WorkMinutesPerDay > 0 ? salaryPerDay / payrollSettings.WorkMinutesPerDay : 0m;
                    decimal bonus = (baseSalary * payrollSettings.BonusRate) + payrollSettings.BonusFixedAmount;
                    decimal overtimePay = salaryPerMinute * overtimeMinutes * payrollSettings.OvertimeMultiplier;
                    decimal unpaidLeaveDeduction = salaryPerDay * leaveDays;
                    decimal insuranceDeduction = baseSalary * payrollSettings.InsuranceRate;
                    decimal lateDeduction = salaryPerMinute * lateMinutes;
                    decimal deduction = unpaidLeaveDeduction + insuranceDeduction + lateDeduction;
                    decimal grossSalary = baseSalary + bonus + overtimePay;
                    decimal netSalary = grossSalary - deduction;

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
                        OvertimePay = Math.Round(overtimePay, 2),
                        Bonus = Math.Round(bonus, 2),
                        Deduction = Math.Round(deduction, 2),
                        NetSalary = Math.Round(netSalary, 2),
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

        public int UpdateStatus(int month, int year, string fromStatus, string toStatus)
        {
            using (var context = new HrmanagementSystemContext())
            {
                var payrolls = context.Payrolls
                    .Where(p => p.Month == month
                                && p.Year == year
                                && p.Status != null
                                && p.Status.Equals(fromStatus))
                    .ToList();

                foreach (var payroll in payrolls)
                {
                    payroll.Status = toStatus;
                }

                return context.SaveChanges();
            }
        }

        public int UpdateStatus(int month, int year, int employeeId, string status)
        {
            using (var context = new HrmanagementSystemContext())
            {
                var payrolls = context.Payrolls
                    .Where(p => p.Month == month && p.Year == year && p.EmployeeId == employeeId)
                    .ToList();

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

        private static int CountLateMinutes(DateTime checkIn, TimeSpan workStartTime, int graceMinutes)
        {
            var effectiveStart = workStartTime.Add(TimeSpan.FromMinutes(Math.Max(0, graceMinutes)));
            var late = checkIn.TimeOfDay - effectiveStart;
            if (late <= TimeSpan.Zero)
            {
                return 0;
            }

            return (int)Math.Ceiling(late.TotalMinutes);
        }

        private static int CountOvertimeMinutes(DateTime checkOut, TimeSpan workEndTime)
        {
            var overtime = checkOut.TimeOfDay - workEndTime;
            if (overtime <= TimeSpan.Zero)
            {
                return 0;
            }

            return (int)Math.Ceiling(overtime.TotalMinutes);
        }

        private static PayrollSettings LoadPayrollSettings(HrmanagementSystemContext context, int fallbackStandardWorkDays)
        {
            var settings = context.Settings.AsNoTracking()
                .ToDictionary(s => s.SettingKey, s => s.SettingValue, StringComparer.OrdinalIgnoreCase);

            TimeSpan workStart = ParseTimeOrDefault(settings, "WORK_START_TIME", new TimeSpan(8, 0, 0));
            TimeSpan workEnd = ParseTimeOrDefault(settings, "WORK_END_TIME", new TimeSpan(17, 0, 0));
            int lateGrace = ParseIntOrDefault(settings, "LATE_GRACE_MINUTES", 5);
            decimal insuranceRate = ParseDecimalOrDefault(settings, "INSURANCE_RATE", 0m);
            decimal overtimeMultiplier = ParseDecimalOrDefault(settings, "OVERTIME_MULTIPLIER", 1.5m);
            decimal bonusRate = ParseDecimalOrDefault(settings, "BONUS_RATE", 0m);
            decimal bonusFixedAmount = ParseDecimalOrDefault(settings, "BONUS_FIXED", 0m);

            int standardDays = ParseIntOrDefault(settings, "STANDARD_WORK_DAYS", fallbackStandardWorkDays);
            if (standardDays <= 0)
            {
                standardDays = fallbackStandardWorkDays > 0 ? fallbackStandardWorkDays : 26;
            }

            int workMinutesPerDay = (int)(workEnd - workStart).TotalMinutes;
            if (workMinutesPerDay <= 0)
            {
                workMinutesPerDay = 9 * 60;
            }

            return new PayrollSettings
            {
                WorkStartTime = workStart,
                WorkEndTime = workEnd,
                LateGraceMinutes = Math.Max(0, lateGrace),
                InsuranceRate = insuranceRate < 0m ? 0m : insuranceRate,
                OvertimeMultiplier = overtimeMultiplier <= 0m ? 1m : overtimeMultiplier,
                BonusRate = bonusRate < 0m ? 0m : bonusRate,
                BonusFixedAmount = bonusFixedAmount < 0m ? 0m : bonusFixedAmount,
                StandardWorkDays = standardDays,
                WorkMinutesPerDay = workMinutesPerDay
            };
        }

        private static TimeSpan ParseTimeOrDefault(
            Dictionary<string, string> settings,
            string key,
            TimeSpan defaultValue)
        {
            if (settings.TryGetValue(key, out var raw) && TimeSpan.TryParse(raw, out var value))
            {
                return value;
            }

            return defaultValue;
        }

        private static int ParseIntOrDefault(
            Dictionary<string, string> settings,
            string key,
            int defaultValue)
        {
            if (settings.TryGetValue(key, out var raw) && int.TryParse(raw, out var value))
            {
                return value;
            }

            return defaultValue;
        }

        private static decimal ParseDecimalOrDefault(
            Dictionary<string, string> settings,
            string key,
            decimal defaultValue)
        {
            if (!settings.TryGetValue(key, out var raw))
            {
                return defaultValue;
            }

            if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var invariantValue))
            {
                return invariantValue;
            }

            if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.CurrentCulture, out var localValue))
            {
                return localValue;
            }

            return defaultValue;
        }

        private sealed class PayrollSettings
        {
            public TimeSpan WorkStartTime { get; set; }
            public TimeSpan WorkEndTime { get; set; }
            public int LateGraceMinutes { get; set; }
            public decimal InsuranceRate { get; set; }
            public decimal OvertimeMultiplier { get; set; }
            public decimal BonusRate { get; set; }
            public decimal BonusFixedAmount { get; set; }
            public int StandardWorkDays { get; set; }
            public int WorkMinutesPerDay { get; set; }
        }
    }
}
