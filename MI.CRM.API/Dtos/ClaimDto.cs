namespace MI.CRM.API.Dtos
{
    public class ClaimDto
    {
        public int ProjectId { get; set; }
        public int ClaimNumber { get; set; }
        public decimal DisbursedAmount { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public DateTime DisbursementDate { get; set; }
    }
}
