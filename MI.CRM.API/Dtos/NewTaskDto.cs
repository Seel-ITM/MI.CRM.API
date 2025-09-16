namespace MI.CRM.API.Dtos
{
    public class NewTaskDto
    {
        public int ProjectId { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? AssignedTo { get; set; }
        public int StatusId { get; set; }
        public int ActivityTypeId { get; set; }
        public string DeliverableType { get; set; }
    }

}
