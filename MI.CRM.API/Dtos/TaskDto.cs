namespace MI.CRM.API.Dtos
{
    public class TaskDto
    {
        public int Id { get; set; }

        public required string Title { get; set; }

        public string? Description { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int? AssignedTo { get; set; }

        public string AssigneeName { get; set; }
        public int StatusId { get; set; }

        public required string StatusName { get; set; }
        public string? StatusColor { get; set; }

        public int ActivityTypeId { get; set; }

        public required string ActivityTypeName { get; set; }

        public string? DeliverableType { get; set; }

        public DateTime? CreatedOn { get; set; }

        public int? CreatedBy { get; set; }
        public DateTime? CompletedOn { get; set; }
        public int? CompletedBy { get; set; }
        public string CompletedByName { get; set; }
    }

}
