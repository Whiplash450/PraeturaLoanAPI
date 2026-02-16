namespace PraeturaLoanAPI.Models
{
    public class CreateLoanApplication
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public decimal MonthlyIncome { get; set; }
        public decimal RequestedAmount { get; set; }
        public int TermMonths { get; set; }
    }
}
