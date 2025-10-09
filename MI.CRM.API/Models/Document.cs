using System;
using System.Collections.Generic;

namespace MI.CRM.API.Models;

public partial class Document
{
    public int Id { get; set; }

    public string DocumentUrl { get; set; } = null!;

    public string DocumentName { get; set; } = null!;

    public int ProjectId { get; set; }

    public int UploadedBy { get; set; }

    public DateTime UploadedAt { get; set; }

    public int? DeletedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual User? DeletedByNavigation { get; set; }

    public virtual ICollection<DisbursementLog> DisbursementLogs { get; set; } = new List<DisbursementLog>();

    public virtual Project Project { get; set; } = null!;

    public virtual User UploadedByNavigation { get; set; } = null!;
}
