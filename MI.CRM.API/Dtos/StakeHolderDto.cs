namespace MI.CRM.API.Dtos
{
    public class StakeHolderDto
    {
        public ProjectMangerDto ProjectManageer { get; set; } = null!;
        public SubcontractorDto Subcontractor { get; set; } = null!;
    }

    public class ProjectMangerDto { 
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string? Name { get; set; } = null!;
        public string? Email { get; set; } = null!;
    }

    public class  SubcontractorDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
    }
}
