using System;
using System.Collections.Generic;

namespace MI.CRM.API.Models;

public partial class Role
{
    public int RoleId { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<ProjectManager> ProjectManagers { get; set; } = new List<ProjectManager>();

    public virtual ICollection<SubContractor> SubContractors { get; set; } = new List<SubContractor>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
