namespace MI.CRM.API.Dtos
{
    public class OverviewRequestDto
    {
        public int ProjectId { get; set; }
        public DateTime WeekStartDate { get; set; }
        public DateTime WeekEndDate { get; set; }
    }
}
