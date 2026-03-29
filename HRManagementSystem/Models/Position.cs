using System;
using System.Collections.Generic;

namespace HRManagementSystem.Models;

public partial class Position
{
    public int PositionId { get; set; }

    public string PositionName { get; set; } = null!;

    public decimal? BaseSalary { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
