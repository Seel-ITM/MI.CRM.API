namespace MI.CRM.API.Dtos
{
    public class ProjectDto
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
        public int? TotalApprovedBudget { get; set; }
        public int? TotalDisbursedBudget { get; set; }
        public int? TotalRemainingBudget { get; set; }
    }

}
