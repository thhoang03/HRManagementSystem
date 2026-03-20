using System;

namespace HRManagementSystem.Models
{
    public sealed class PayrollPreview
    {
        public int? PayrollId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public int? ContractId { get; set; }
        public string? ContractType { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal BaseSalary { get; set; }
        public int WorkDays { get; set; }
        public int LeaveDays { get; set; }
        public decimal OvertimePay { get; set; }
        public decimal Bonus { get; set; }
        public decimal Deduction { get; set; }
        public decimal NetSalary { get; set; }
        public string Status { get; set; } = "Draft";
        public DateTime? CreatedAt { get; set; }
    }
}
