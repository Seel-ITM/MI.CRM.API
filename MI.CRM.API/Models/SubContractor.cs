using System;
using System.Collections.Generic;

namespace MI.CRM.API.Models;

public partial class SubContractor
{
    public int SubContractorId { get; set; }

    public string Name { get; set; } = null!;

    public string? Email { get; set; }

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}
