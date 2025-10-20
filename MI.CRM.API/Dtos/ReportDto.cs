namespace MI.CRM.API.Dtos
{
    public class ReportDto
    {
        
        public ProjectDto Project { get; set; }
        public Dictionary<string, string> ProjectOverview { get; set; } = new Dictionary<string, string>();
        public List<TaskDto> Tasks {  get; set; }
        public List<StakeHolderDto> StakeHolders { get; set; }
        public List<ProjectBudgetEntryDto> ProjectBudgetEntries { get; set; }
    }


}
