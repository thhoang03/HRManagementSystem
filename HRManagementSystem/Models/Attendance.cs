using System;
using System.Collections.Generic;

namespace HRManagementSystem.Models;

public partial class Attendance
{
    public int AttendanceId { get; set; }

    public int? EmployeeId { get; set; }

    public DateOnly? AttendanceDate { get; set; }

    public DateTime? CheckIn { get; set; }

    public DateTime? CheckOut { get; set; }

    public string? DeviceIp { get; set; }

    public string? Status { get; set; }

    public virtual Employee? Employee { get; set; }
}
