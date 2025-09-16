namespace MI.CRM.API.Dtos
{
    public class MainPageDataDto
    {
        public List<ProjectDto> Projects { get; set; }
        public int TotalProjects { get; set; }
        public int States { get; set; }
        public decimal TotalApprovedBudget { get; set; }
    }
}
