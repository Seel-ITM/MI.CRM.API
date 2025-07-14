using System;
using System.Collections.Generic;

namespace MI.CRM.API.Models;

public partial class Document
{
    public int DocumentId { get; set; }

    public int? TaskId { get; set; }

    public int? ProjectId { get; set; }

    public string? DocumentName { get; set; }

    public string? DocumentUrl { get; set; }

    public virtual Project? Project { get; set; }

    public virtual Task? Task { get; set; }
}
