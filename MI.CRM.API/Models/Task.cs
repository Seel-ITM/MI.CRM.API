using System;
using System.Collections.Generic;

namespace MI.CRM.API.Models;

public partial class Task
{
    public int TaskId { get; set; }

    public string? DeliverableType { get; set; }

    public int? BudgetedEvents { get; set; }

    public int? CompletedEvents { get; set; }

    public int? OutstandingEvents { get; set; }

    public int? CompletedDeliverables { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public int? ProjectId { get; set; }

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual Project? Project { get; set; }
}
