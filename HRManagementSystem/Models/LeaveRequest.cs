using System;
using System.Collections.Generic;

namespace HRManagementSystem.Models;

public partial class LeaveRequest
{
    public int LeaveId { get; set; }

    public int? EmployeeId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string? LeaveType { get; set; }

    public string? Reason { get; set; }

    public string? Status { get; set; }

    public int? ApprovedBy { get; set; }

    public virtual Employee? ApprovedByNavigation { get; set; }

    public virtual Employee? Employee { get; set; }
}
