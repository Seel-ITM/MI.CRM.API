using System;
using System.Collections.Generic;

namespace MI.CRM.API.Models;

public partial class ProjectBudgetEntry
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public string AwardNumber { get; set; } = null!;

    public int CategoryId { get; set; }

    public int TypeId { get; set; }

    public decimal Amount { get; set; }

    public string? Notes { get; set; }

    public virtual BudgetCategory Category { get; set; } = null!;

    public virtual Project Project { get; set; } = null!;

    public virtual BudgetType Type { get; set; } = null!;
}
