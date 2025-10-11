namespace MI.CRM.API.Dtos
{
    public class UserUpdateDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public int RoleId { get; set; }
        public IFormFile? ImageFile { get; set; } // same as register
    }
}
