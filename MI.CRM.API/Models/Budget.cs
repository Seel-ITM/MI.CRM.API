using System;
using System.Collections.Generic;

namespace MI.CRM.API.Models;

public partial class Budget
{
    public int Id { get; set; }

    public decimal? Personal { get; set; }

    public decimal? FringeBenefits { get; set; }

    public decimal? Travel { get; set; }

    public decimal? Equipment { get; set; }

    public decimal? Supplies { get; set; }

    public decimal? Contractual { get; set; }

    public decimal? Construnction { get; set; }

    public decimal? Other { get; set; }

    public decimal? TotalDirectCharges { get; set; }

    public decimal? IndirectCharges { get; set; }

    public decimal? Totals { get; set; }

    public string? BudgetType { get; set; }

    public int? ProjectId { get; set; }

    public virtual Project? Project { get; set; }
}
