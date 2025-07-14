using System;
using System.Collections.Generic;

namespace MI.CRM.API.Models;

public partial class User
{
    public int UserId { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Details { get; set; }

    public int? RoleId { get; set; }

    public virtual ICollection<ProjectManager> ProjectManagers { get; set; } = new List<ProjectManager>();

    public virtual Role? Role { get; set; }

    public virtual ICollection<SubContractor> SubContractors { get; set; } = new List<SubContractor>();
}
