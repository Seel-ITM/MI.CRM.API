namespace MI.CRM.API.Dtos
{
    public class OverviewResponseDto
    {
        public int ProjectId { get; set; }
        public int ActiveTasks { get; set; }
        public int UpcomingTasks { get; set; }
        public int PendingTasks { get; set; }
        public int BudgetPercentageUsed { get; set; }

        public int ToDoTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int RemainingTasks { get; set; }
        public int ProgressPercentage { get; set; }

        public List<TaskNotificationDto> Notifications { get; set; } = new();
    }

    public class TaskNotificationDto
    {
        public string Title { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public string TimeRemaining { get; set; } = string.Empty;
    }

    public class RecentUpdatesDto
    {

    }
}
