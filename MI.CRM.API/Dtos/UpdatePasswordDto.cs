namespace MI.CRM.API.Dtos
{
    public class UpdatePasswordDto
    {
        public int UserId { get; set; }
        public string NewPassword { get; set; } = string.Empty;
    }
}
