namespace PraeturaLoanAPI.Models
{
    public class DecisionLogEntry
    {
        public Guid Id { get; set; }
        public Guid LoanApplicationId { get; set; }
        public string RuleName { get; set; }
        public bool Passed { get; set; }
        public string Message { get; set; }
        public DateTime EvaluatedAt { get; set; }
    }
}
