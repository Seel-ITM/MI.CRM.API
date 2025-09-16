using System;
using System.Collections.Generic;

namespace MI.CRM.API.Models;

public partial class Task
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int ActivityTypeId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int? AssignedTo { get; set; }

    public int StatusId { get; set; }

    public string? DeliverableType { get; set; }

    public DateTime? CreatedOn { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CompletedOn { get; set; }

    public int? CompletedBy { get; set; }

    public virtual ActivityType ActivityType { get; set; } = null!;

    public virtual User? AssignedToNavigation { get; set; }

    public virtual User? CompletedByNavigation { get; set; }

    public virtual Project Project { get; set; } = null!;

    public virtual TaskStatus Status { get; set; } = null!;

    public virtual ICollection<TaskLog> TaskLogs { get; set; } = new List<TaskLog>();
}
