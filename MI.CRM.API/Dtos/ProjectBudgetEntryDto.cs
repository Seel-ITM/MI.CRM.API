namespace MI.CRM.API.Dtos
{
    public class ProjectBudgetEntryDto
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string AwardNumber { get; set; } = null!;
        public int CategoryId { get; set; }
        public int TypeId { get; set; }
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
        public string? CategoryName { get; set; }
        public string? TypeName { get; set; }
        public List<DisbursementDto> Disbursements { get; set; }  = new List<DisbursementDto>();
    }
}
