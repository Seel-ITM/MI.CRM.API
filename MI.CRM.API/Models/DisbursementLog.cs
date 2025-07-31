using System;
using System.Collections.Generic;

namespace MI.CRM.API.Models;

public partial class DisbursementLog
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public int CategoryId { get; set; }

    public int UserId { get; set; }

    public decimal DisbursedAmount { get; set; }

    public string? Description { get; set; }

    public int? DocumentId { get; set; }

    public DateTime DisbursementDate { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual BudgetCategory Category { get; set; } = null!;

    public virtual Document? Document { get; set; }

    public virtual Project Project { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
