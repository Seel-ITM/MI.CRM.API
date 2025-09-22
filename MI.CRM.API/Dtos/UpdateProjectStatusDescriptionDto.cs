namespace MI.CRM.API.Dtos
{
    public class UpdateProjectStatusDescriptionDto
    {
        public int ProjectId { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
