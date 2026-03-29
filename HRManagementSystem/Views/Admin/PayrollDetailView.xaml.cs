using HRManagementSystem.BLL;
using HRManagementSystem.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace HRManagementSystem.Views.Admin
{
    /// <summary>
    /// Interaction logic for PayrollDetailView.xaml
    /// </summary>
    public partial class PayrollDetailView : Page
    {
        private readonly PayrollBLL _payBLL = new();
        private PayrollPreview _payroll;
        private PayrollSettings _settings = new();
        private PayrollDetailMetrics _metrics = new();

        public PayrollDetailView(PayrollPreview payroll)
        {
            InitializeComponent();
            _payroll = payroll;
            LoadDetail();
        }

        private void LoadDetail()
        {
            _settings = LoadSettings();
            _metrics = BuildMetrics(_payroll, _settings);

            txtEmployeeName.Text = _payroll.EmployeeName;
            txtPayrollId.Text = _payroll.PayrollId?.ToString() ?? "(Preview)";
            txtMonthYear.Text = $"{_payroll.Month:00}/{_payroll.Year}";
            txtStatus.Text = string.IsNullOrWhiteSpace(_payroll.Status) ? "N/A" : _payroll.Status;
            txtContractType.Text = string.IsNullOrWhiteSpace(_payroll.ContractType) ? "N/A" : _payroll.ContractType;
            txtContractId.Text = _metrics.ContractCode;
            txtCreatedAt.Text = _payroll.CreatedAt?.ToString("g") ?? "N/A";

            txtBaseSalary.Text = FormatMoney(_payroll.BaseSalary);
            txtSalaryPerDay.Text = FormatMoney(_metrics.SalaryPerDay);
            txtBonus.Text = FormatMoney(_payroll.Bonus);
            txtOvertime.Text = FormatMoney(_payroll.OvertimePay);
            txtGrossSalary.Text = FormatMoney(_metrics.GrossSalary);
            txtNetSalary.Text = FormatMoney(_payroll.NetSalary);

            txtStandardDays.Text = _settings.StandardWorkDays.ToString(CultureInfo.InvariantCulture);
            txtActualDays.Text = _metrics.ActualWorkDays.ToString(CultureInfo.InvariantCulture);
            txtPaidLeaveDays.Text = _metrics.PaidLeaveDays.ToString(CultureInfo.InvariantCulture);
            txtUnpaidLeaveDays.Text = _metrics.UnpaidLeaveDays.ToString(CultureInfo.InvariantCulture);
            txtLateCount.Text = _metrics.LateCount.ToString(CultureInfo.InvariantCulture);
            txtLateMinutes.Text = _metrics.TotalLateMinutes.ToString(CultureInfo.InvariantCulture);
            dgReconcile.ItemsSource = _metrics.ReconcileItems;

            txtInsuranceDeduction.Text = FormatMoney(_metrics.InsuranceDeduction);
            txtUnpaidLeaveDeduction.Text = FormatMoney(_metrics.UnpaidLeaveDeduction);
            txtLateDeduction.Text = FormatMoney(_metrics.LateDeduction);
            txtDeductionBonus.Text = FormatMoney(_payroll.Bonus);
            txtDeductionOvertime.Text = FormatMoney(_payroll.OvertimePay);
            txtTotalDeduction.Text = FormatMoney(_payroll.Deduction);

            txtWorkStart.Text = _settings.WorkStartTime.ToString(@"hh\:mm\:ss");
            txtWorkEnd.Text = _settings.WorkEndTime.ToString(@"hh\:mm\:ss");
            txtLateGrace.Text = _settings.LateGraceMinutes.ToString(CultureInfo.InvariantCulture);
            txtInsuranceRate.Text = _settings.InsuranceRate.ToString("P2", CultureInfo.InvariantCulture);
            txtSettingWorkDays.Text = _settings.StandardWorkDays.ToString(CultureInfo.InvariantCulture);
            txtOvertimeExplanation.Text = BuildOvertimeExplanation();
            dgOvertimeExplain.ItemsSource = _metrics.OvertimeItems;

            btnApprovePayroll.Visibility = IsStatus(_payroll.Status, "Draft")
                ? Visibility.Visible
                : Visibility.Collapsed;
            btnPayPayroll.Visibility = IsStatus(_payroll.Status, "Approved")
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
            {
                NavigationService.GoBack();
                return;
            }

            if (Window.GetWindow(this) is MainAdmin mainAdmin)
            {
                mainAdmin.MainFrameControl.Navigate(new PayrollView());
            }
        }

        private void btnRecalculate_Click(object sender, RoutedEventArgs e)
        {
            var previews = _payBLL.GeneratePreview(_payroll.Month, _payroll.Year);
            var refreshed = previews.FirstOrDefault(p => p.EmployeeId == _payroll.EmployeeId);
            if (refreshed == null)
            {
                MessageBox.Show("Cannot recalculate this payroll row.", "Payroll", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            refreshed.PayrollId = _payroll.PayrollId;
            refreshed.Status = _payroll.Status;
            refreshed.CreatedAt = _payroll.CreatedAt;

            _payroll = refreshed;
            LoadDetail();
            MessageBox.Show("Payroll has been recalculated from latest attendance/leave/contract data.", "Payroll", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                FileName = $"Payslip_{_payroll.EmployeeName}_{_payroll.Month:00}_{_payroll.Year}.csv"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            var lines = new List<string>
            {
                "Field,Value",
                $"Employee,{EscapeCsv(_payroll.EmployeeName)}",
                $"MonthYear,{_payroll.Month:00}/{_payroll.Year}",
                $"Status,{EscapeCsv(_payroll.Status)}",
                $"ContractType,{EscapeCsv(_payroll.ContractType)}",
                $"ContractID,{EscapeCsv(_metrics.ContractCode)}",
                $"BaseSalary,{_payroll.BaseSalary}",
                $"Bonus,{_payroll.Bonus}",
                $"OvertimePay,{_payroll.OvertimePay}",
                $"InsuranceDeduction,{_metrics.InsuranceDeduction}",
                $"UnpaidLeaveDeduction,{_metrics.UnpaidLeaveDeduction}",
                $"LateDeduction,{_metrics.LateDeduction}",
                $"TotalDeduction,{_payroll.Deduction}",
                $"NetSalary,{_payroll.NetSalary}"
            };

            File.WriteAllLines(dialog.FileName, lines, Encoding.UTF8);
            MessageBox.Show("Payslip exported successfully.", "Payroll", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnApprovePayroll_Click(object sender, RoutedEventArgs e)
        {
            if (!IsStatus(_payroll.Status, "Draft"))
            {
                MessageBox.Show("Only Draft payroll can be approved.", "Payroll", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show(
                $"Approve payroll for {_payroll.EmployeeName} ({_payroll.Month:00}/{_payroll.Year})?",
                "Confirm Approve Payroll",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            if (!_payBLL.HasPayroll(_payroll.Month, _payroll.Year, _payroll.EmployeeId))
            {
                _payBLL.SavePayrolls(new[] { _payroll });
            }

            int affected = _payBLL.UpdateStatus(_payroll.Month, _payroll.Year, _payroll.EmployeeId, "Approved");
            if (affected <= 0)
            {
                MessageBox.Show("Approve failed. Payroll not found.", "Payroll", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _payroll.Status = "Approved";
            LoadDetail();
            MessageBox.Show("Payroll approved successfully.", "Payroll", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnPayPayroll_Click(object sender, RoutedEventArgs e)
        {
            if (!IsStatus(_payroll.Status, "Approved"))
            {
                MessageBox.Show("Only Approved payroll can be paid.", "Payroll", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show(
                $"Pay payroll for {_payroll.EmployeeName} ({_payroll.Month:00}/{_payroll.Year})?",
                "Confirm Pay Payroll",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            if (!_payBLL.HasPayroll(_payroll.Month, _payroll.Year, _payroll.EmployeeId))
            {
                _payBLL.SavePayrolls(new[] { _payroll });
            }

            int affected = _payBLL.UpdateStatus(_payroll.Month, _payroll.Year, _payroll.EmployeeId, "Paid");
            if (affected <= 0)
            {
                MessageBox.Show("Pay failed. Payroll not found.", "Payroll", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _payroll.Status = "Paid";
            LoadDetail();
            MessageBox.Show("Payroll paid successfully.", "Payroll", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private PayrollDetailMetrics BuildMetrics(PayrollPreview payroll, PayrollSettings settings)
        {
            using var context = new HrmanagementSystemContext();

            var monthStart = new DateTime(payroll.Year, payroll.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var attendances = context.Attendances
                .Where(a => a.EmployeeId == payroll.EmployeeId
                            && a.AttendanceDate.HasValue
                            && a.AttendanceDate.Value.Month == payroll.Month
                            && a.AttendanceDate.Value.Year == payroll.Year)
                .ToList();

            var approvedLeaves = context.LeaveRequests
                .Where(l => l.EmployeeId == payroll.EmployeeId
                            && l.Status != null
                            && l.Status.ToLower() == "approved"
                            && l.StartDate <= monthEnd
                            && l.EndDate >= monthStart)
                .ToList();

            var contract = context.Contracts
                .Where(c => c.EmployeeId == payroll.EmployeeId)
                .OrderByDescending(c => c.ContractId)
                .FirstOrDefault();

            int actualWorkDays = attendances.Count(a => !IsAbsentOrLeave(a.Status));
            int unpaidLeaveDays = approvedLeaves
                .Where(l => IsUnpaidLeave(l.LeaveType))
                .Sum(l => CountOverlapDays(l.StartDate, l.EndDate, monthStart, monthEnd));
            int paidLeaveDays = approvedLeaves
                .Where(l => !IsUnpaidLeave(l.LeaveType))
                .Sum(l => CountOverlapDays(l.StartDate, l.EndDate, monthStart, monthEnd));

            var lateItems = attendances
                .Where(a => a.CheckIn.HasValue)
                .Select(a => new
                {
                    Date = a.AttendanceDate!.Value.ToDateTime(TimeOnly.MinValue),
                    LateMinutes = CountLateMinutes(a.CheckIn!.Value, settings.WorkStartTime, settings.LateGraceMinutes),
                    Detail = $"Check in at {a.CheckIn.Value:HH:mm}" 
                })
                .Where(x => x.LateMinutes > 0)
                .ToList();

            int lateCount = lateItems.Count;
            int totalLateMinutes = lateItems.Sum(x => x.LateMinutes);

            decimal salaryPerDay = settings.StandardWorkDays > 0 ? payroll.BaseSalary / settings.StandardWorkDays : 0m;
            decimal salaryPerMinute = settings.WorkMinutesPerDay > 0 ? salaryPerDay / settings.WorkMinutesPerDay : 0m;

            decimal insuranceDeduction = payroll.BaseSalary * settings.InsuranceRate;
            decimal unpaidLeaveDeduction = salaryPerDay * unpaidLeaveDays;
            decimal lateDeduction = salaryPerMinute * totalLateMinutes;
            decimal grossSalary = payroll.BaseSalary + payroll.Bonus + payroll.OvertimePay;
            var overtimeItems = attendances
                .Where(a => a.CheckOut.HasValue)
                .Select(a => new OvertimeExplainItem
                {
                    Date = a.AttendanceDate?.ToDateTime(TimeOnly.MinValue) ?? DateTime.MinValue,
                    CheckOutTime = a.CheckOut!.Value.ToString("HH:mm"),
                    OvertimeMinutes = CountOvertimeMinutes(a.CheckOut!.Value, settings.WorkEndTime)
                })
                .Where(o => o.OvertimeMinutes > 0)
                .OrderBy(o => o.Date)
                .ToList();

            var reconcileItems = new List<ReconcileItem>();
            foreach (var leave in approvedLeaves.Where(l => IsUnpaidLeave(l.LeaveType)))
            {
                DateTime start = leave.StartDate.Date < monthStart ? monthStart : leave.StartDate.Date;
                DateTime end = leave.EndDate.Date > monthEnd ? monthEnd : leave.EndDate.Date;

                for (DateTime d = start; d <= end; d = d.AddDays(1))
                {
                    reconcileItems.Add(new ReconcileItem
                    {
                        Date = d,
                        Type = "Unpaid Leave",
                        LateMinutes = 0,
                        Reason = string.IsNullOrWhiteSpace(leave.Reason)
                            ? (leave.LeaveType ?? "N/A")
                            : leave.Reason
                    });
                }
            }

            reconcileItems.AddRange(lateItems.Select(l => new ReconcileItem
            {
                Date = l.Date,
                Type = "Late Arrival",
                LateMinutes = l.LateMinutes,
                Reason = l.Detail
            }));

            return new PayrollDetailMetrics
            {
                ContractCode = contract == null
                    ? "N/A"
                    : $"CNT-{contract.StartDate.Year}-{contract.ContractId:000}",
                ActualWorkDays = actualWorkDays,
                PaidLeaveDays = paidLeaveDays,
                UnpaidLeaveDays = unpaidLeaveDays,
                LateCount = lateCount,
                TotalLateMinutes = totalLateMinutes,
                SalaryPerDay = salaryPerDay,
                InsuranceDeduction = insuranceDeduction,
                UnpaidLeaveDeduction = unpaidLeaveDeduction,
                LateDeduction = lateDeduction,
                GrossSalary = grossSalary,
                OvertimeItems = overtimeItems,
                ReconcileItems = reconcileItems
                    .OrderBy(r => r.Date)
                    .ThenBy(r => r.Type)
                    .ToList()
            };
        }

        private string BuildOvertimeExplanation()
        {
            if (_payroll.OvertimePay <= 0m)
            {
                return "No overtime pay applied in this payroll period.";
            }

            int otMinutes = _metrics.OvertimeItems.Sum(o => o.OvertimeMinutes);
            if (otMinutes <= 0)
            {
                return $"Overtime pay {FormatMoney(_payroll.OvertimePay)} exists in payroll record, but no overtime attendance detail was detected for this period.";
            }

            decimal otRatePerMinute = _payroll.OvertimePay / otMinutes;
            return $"Overtime pay {FormatMoney(_payroll.OvertimePay)} is mapped from {otMinutes} overtime minutes (after {_settings.WorkEndTime.ToString(@"hh\:mm\:ss")}), approximate rate {FormatMoney(otRatePerMinute)} per minute.";
        }

        private static PayrollSettings LoadSettings()
        {
            using var context = new HrmanagementSystemContext();
            var dict = context.Settings
                .ToDictionary(s => s.SettingKey, s => s.SettingValue, StringComparer.OrdinalIgnoreCase);

            TimeSpan workStart = ParseTime(dict, "WORK_START_TIME", new TimeSpan(8, 0, 0));
            TimeSpan workEnd = ParseTime(dict, "WORK_END_TIME", new TimeSpan(17, 0, 0));
            int workMinutes = (int)(workEnd - workStart).TotalMinutes;
            if (workMinutes <= 0)
            {
                workMinutes = 9 * 60;
            }

            return new PayrollSettings
            {
                WorkStartTime = workStart,
                WorkEndTime = workEnd,
                LateGraceMinutes = Math.Max(0, ParseInt(dict, "LATE_GRACE_MINUTES", 5)),
                InsuranceRate = Math.Max(0m, ParseDecimal(dict, "INSURANCE_RATE", 0.08m)),
                StandardWorkDays = Math.Max(1, ParseInt(dict, "STANDARD_WORK_DAYS", 26)),
                WorkMinutesPerDay = workMinutes
            };
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

        private static string FormatMoney(decimal value)
        {
            return value.ToString("N2", CultureInfo.InvariantCulture);
        }

        private static bool IsStatus(string? current, string expected)
        {
            return string.Equals(current?.Trim(), expected, StringComparison.OrdinalIgnoreCase);
        }

        private static string EscapeCsv(string? value)
        {
            string safe = value ?? string.Empty;
            if (safe.Contains(',') || safe.Contains('"') || safe.Contains('\n'))
            {
                return '"' + safe.Replace("\"", "\"\"") + '"';
            }

            return safe;
        }

        private static TimeSpan ParseTime(Dictionary<string, string> dict, string key, TimeSpan fallback)
        {
            if (dict.TryGetValue(key, out var raw) && TimeSpan.TryParse(raw, out var value))
            {
                return value;
            }

            return fallback;
        }

        private static int ParseInt(Dictionary<string, string> dict, string key, int fallback)
        {
            if (dict.TryGetValue(key, out var raw) && int.TryParse(raw, out var value))
            {
                return value;
            }

            return fallback;
        }

        private static decimal ParseDecimal(Dictionary<string, string> dict, string key, decimal fallback)
        {
            if (!dict.TryGetValue(key, out var raw))
            {
                return fallback;
            }

            if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var inv))
            {
                return inv;
            }

            if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.CurrentCulture, out var local))
            {
                return local;
            }

            return fallback;
        }

        private sealed class PayrollSettings
        {
            public TimeSpan WorkStartTime { get; set; }
            public TimeSpan WorkEndTime { get; set; }
            public int LateGraceMinutes { get; set; }
            public decimal InsuranceRate { get; set; }
            public int StandardWorkDays { get; set; }
            public int WorkMinutesPerDay { get; set; }
        }

        private sealed class PayrollDetailMetrics
        {
            public string ContractCode { get; set; } = "N/A";
            public int ActualWorkDays { get; set; }
            public int PaidLeaveDays { get; set; }
            public int UnpaidLeaveDays { get; set; }
            public int LateCount { get; set; }
            public int TotalLateMinutes { get; set; }
            public decimal SalaryPerDay { get; set; }
            public decimal InsuranceDeduction { get; set; }
            public decimal UnpaidLeaveDeduction { get; set; }
            public decimal LateDeduction { get; set; }
            public decimal GrossSalary { get; set; }
            public List<OvertimeExplainItem> OvertimeItems { get; set; } = new();
            public List<ReconcileItem> ReconcileItems { get; set; } = new();
        }

        private sealed class OvertimeExplainItem
        {
            public DateTime Date { get; set; }
            public string CheckOutTime { get; set; } = string.Empty;
            public int OvertimeMinutes { get; set; }
        }

        private sealed class ReconcileItem
        {
            public DateTime Date { get; set; }
            public string Type { get; set; } = string.Empty;
            public int LateMinutes { get; set; }
            public string Reason { get; set; } = string.Empty;
        }
    }
}
