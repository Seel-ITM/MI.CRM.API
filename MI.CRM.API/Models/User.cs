using System;
using System.Collections.Generic;

namespace MI.CRM.API.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int RoleId { get; set; }

    public DateTime CreatedOn { get; set; }

    public int? CreatedBy { get; set; }

    public string? ImageUrl { get; set; }

    public virtual ICollection<DisbursementLog> DisbursementLogs { get; set; } = new List<DisbursementLog>();

    public virtual ICollection<Document> DocumentDeletedByNavigations { get; set; } = new List<Document>();

    public virtual ICollection<Document> DocumentUploadedByNavigations { get; set; } = new List<Document>();

    public virtual ICollection<ProjectManager> ProjectManagers { get; set; } = new List<ProjectManager>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<Task> TaskAssignedToNavigations { get; set; } = new List<Task>();

    public virtual ICollection<Task> TaskCompletedByNavigations { get; set; } = new List<Task>();

    public virtual ICollection<TaskLog> TaskLogs { get; set; } = new List<TaskLog>();
}
