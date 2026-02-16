namespace PraeturaLoanAPI.Models
{
    public class LoanApplication
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public decimal MonthlyIncome { get; set; }
        public decimal RequestedAmount { get; set; }
        public int TermMonths { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public List<DecisionLogEntry> DecisionLogs { get; set; }
        public string? IdempotencyKey { get; set; }
    }
}
