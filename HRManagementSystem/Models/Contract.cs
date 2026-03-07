using System;
using System.Collections.Generic;

namespace HRManagementSystem.Models;

public partial class Contract
{
    public int ContractId { get; set; }

    public int? EmployeeId { get; set; }

    public string? ContractType { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public decimal? ContractSalary { get; set; }

    public string? Status { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();
}
