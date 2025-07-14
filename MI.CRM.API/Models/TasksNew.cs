using System;
using System.Collections.Generic;

namespace MI.CRM.API.Models;

public partial class TasksNew
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? ActivityType { get; set; }

    public DateTime? DueDate { get; set; }

    public int? AssignedTo { get; set; }

    public string? Status { get; set; }

    public string? ProjectId { get; set; }
}
