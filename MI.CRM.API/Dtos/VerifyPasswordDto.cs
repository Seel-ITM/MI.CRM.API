namespace MI.CRM.API.Dtos
{
    public class VerifyPasswordDto
    {
        public int UserId { get; set; }
        public string CurrentPassword { get; set; } = string.Empty;
    }
}
