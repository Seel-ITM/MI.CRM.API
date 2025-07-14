using System;
using System.Collections.Generic;

namespace MI.CRM.API.Models;

public partial class ProjectManager
{
    public int ProjectManagerId { get; set; }

    public int? RoleId { get; set; }

    public int? UserId { get; set; }

    public string? Details { get; set; }

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

    public virtual Role? Role { get; set; }

    public virtual User? User { get; set; }
}
