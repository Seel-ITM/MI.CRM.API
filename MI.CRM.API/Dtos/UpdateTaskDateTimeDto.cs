namespace MI.CRM.API.Dtos
{
    public class UpdateTaskDateTimeDto
    {
        public int TaskId { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
    }

}
