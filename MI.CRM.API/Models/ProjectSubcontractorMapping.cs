using System;
using System.Collections.Generic;

namespace MI.CRM.API.Models;

public partial class ProjectSubcontractorMapping
{
    public int ProjectId { get; set; }

    public int SubcontractorId { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual Project Project { get; set; } = null!;

    public virtual SubContractor Subcontractor { get; set; } = null!;
}
