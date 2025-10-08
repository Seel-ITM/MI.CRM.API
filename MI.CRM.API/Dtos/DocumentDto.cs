namespace MI.CRM.API.DTOs
{
    public class DocumentDto
    {
        public int Id { get; set; }

        public string DocumentUrl { get; set; } = null!;

        public string DocumentName { get; set; } = null!;

        public int ProjectId { get; set; }

        public int UploadedById { get; set; }

        public string UploadedByName { get; set; } = null!;

        public DateTime UploadedAt { get; set; }
    }
}
