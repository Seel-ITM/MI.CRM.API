using Microsoft.Identity.Client;

namespace MI.CRM.API.Dtos
{
    public class OperationsSummaryDto
    {
        public ProjectDto Project { get; set; }
        public int TotalNumberOfEvents { get; set; }
        public int BudgetedNumberOfEvents { get; set; }
        public int RemainingNumberOfEvents { get; set; }
        public List<string> DeliverableTypes { get; set; }
        public decimal TasksCompletedPercent { get; set; }
        public List<TaskDto> LatestTodoTasks { get; set; }
        public List<TaskDto> LatestFinishedTasks { get; set; }
        public List<TaskDto> Tasks { get; set; }
    }
}
