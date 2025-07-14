using System;
using System.Collections.Generic;

namespace MI.CRM.API.Models;

public partial class BudgetType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<ProjectBudgetEntry> ProjectBudgetEntries { get; set; } = new List<ProjectBudgetEntry>();
}
