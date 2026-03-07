using System;
using System.Collections.Generic;

namespace HRManagementSystem.Models;

public partial class Payroll
{
    public int PayrollId { get; set; }

    public int? ContractId { get; set; }

    public int? EmployeeId { get; set; }

    public int? Month { get; set; }

    public int? Year { get; set; }

    public decimal? BaseSalary { get; set; }

    public decimal? OvertimePay { get; set; }

    public decimal? Deduction { get; set; }

    public decimal? Bonus { get; set; }

    public decimal? NetSalary { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Contract? Contract { get; set; }

    public virtual Employee? Employee { get; set; }
}
