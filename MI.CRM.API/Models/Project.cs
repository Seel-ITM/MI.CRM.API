using System;
using System.Collections.Generic;

namespace MI.CRM.API.Models;

public partial class Project
{
    public int ProjectId { get; set; }

    public string AwardNumber { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Category { get; set; } = null!;

    public string Agency { get; set; } = null!;

    public string Company { get; set; } = null!;

    public string State { get; set; } = null!;

    public int? ProjectManagerId { get; set; }

    public int? SubContractorId { get; set; }

    public decimal? TotalApprovedBudget { get; set; }

    public decimal? TotalDisbursedBudget { get; set; }

    public decimal? TotalRemainingBudget { get; set; }

    public decimal? BilledNotPaid { get; set; }

    public string? Status { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? ProjectStatus { get; set; }

    public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();

    public virtual ICollection<DisbursementLog> DisbursementLogs { get; set; } = new List<DisbursementLog>();

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual ICollection<ProjectBudgetEntry> ProjectBudgetEntryAwardNumberNavigations { get; set; } = new List<ProjectBudgetEntry>();

    public virtual ICollection<ProjectBudgetEntry> ProjectBudgetEntryProjects { get; set; } = new List<ProjectBudgetEntry>();

    public virtual ProjectManager? ProjectManager { get; set; }

    public virtual SubContractor? SubContractor { get; set; }

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
