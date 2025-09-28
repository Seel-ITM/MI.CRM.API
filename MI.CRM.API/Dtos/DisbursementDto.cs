namespace MI.CRM.API.Dtos
{
    public class DisbursementDto
    {
        public int ProjectId { get; set; }
        public int CategoryId { get; set; }
        public string? Description { get; set; }
        public DateTime DisbursementDate { get; set; }
        public decimal DisbursedAmount { get; set; }
        public decimal? Units { get; set; }
        public decimal? Rate { get; set; }
    }

    public class NewDisbursementDto : DisbursementDto{
        public int BudgetEntryId { get; set; }
        public int? DocumentId { get; set; }
        public int? ClaimNumber { get; set; }
    }

}
