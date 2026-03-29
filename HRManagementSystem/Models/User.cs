using System;
using System.Collections.Generic;

namespace HRManagementSystem.Models;

public partial class User
{
    public int UserId { get; set; }

    public int? EmployeeId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? Role { get; set; }

    public string? Status { get; set; }

    public DateTime? LastLogin { get; set; }

    public virtual Employee? Employee { get; set; }
}
