using System;
using System.Collections.Generic;

namespace MI.CRM.API.Models;

public partial class TaskStatus
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Color { get; set; }

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
