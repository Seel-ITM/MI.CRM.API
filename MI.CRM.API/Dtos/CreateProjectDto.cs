namespace MI.CRM.API.Dtos
{
    public class CreateProjectDto
    {
        public ProjectDetailsDto ProjectDetails { get; set; }
        public SubcontractorDetailsDto SubcontractorDetails { get; set; }
        public List<ProjectBudgetInfoDto> ProjectBudgetInfo { get; set; }
    }

    public class ProjectDetailsDto
    {
        public string Title { get; set; }
        public string AwardNumber { get; set; }
        public string Category { get; set; }
        public string Agency { get; set; }
        public string Company { get; set; }
        public string State { get; set; }
        public string ProjectStatus { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class SubcontractorDetailsDto
    {
        public string SubcontractorName { get; set; }
        public string? Email { get; set; } // Nullable to match optional field
    }

    public class ProjectBudgetInfoDto
    {
        public int CategoryId { get; set; } // Change to `string` if that's what your backend uses
        public decimal ApprovedAmount { get; set; } // decimal is preferred for currency values
    }

}
