using PraeturaLoanAPI.Models;

namespace PraeturaLoanAPI.Services
{
    public class EligibilityService : IEligibilityService
    {
        public List<DecisionLogEntry> Evaluate(LoanApplication application)
        {
            var results = new List<DecisionLogEntry>();

            var maxAmount = application.MonthlyIncome * 4;
            var minIncome = 2000;
            var termMin = 12;
            var termMax = 60;

            // First, check min income 
            results.Add(CreateLogEntry(application.Id, "MinimumIncome",
            application.MonthlyIncome >= minIncome,
            "Monthly income meets minimum threshold of £2,000",
            $"Monthly income of £{application.MonthlyIncome} is below minimum threshold of £2,000"));

            // Second, check max loan amount
            results.Add(CreateLogEntry(application.Id, "MaximumAmount",
                application.RequestedAmount <= maxAmount,
                $"Requested amount is within maximum of £{maxAmount}",
                $"Requested amount of £{application.RequestedAmount} exceeds maximum of £{maxAmount}"));

            // Third, validate requested term range
            results.Add(CreateLogEntry(application.Id, "TermRange",
                application.TermMonths >= termMin && application.TermMonths <= termMax,
                $"Term of {application.TermMonths} months is within acceptable range ({termMin}-{termMax})",
                $"Term of {application.TermMonths} months is outside acceptable range ({termMin}-{termMax})"));

            return results;
        }

        private DecisionLogEntry CreateLogEntry(Guid applicationId, string ruleName, bool passed, string passMessage, string failMessage)
        {
            return new DecisionLogEntry
            {
                Id = Guid.NewGuid(),
                LoanApplicationId = applicationId,
                RuleName = ruleName,
                Passed = passed,
                Message = passed ? passMessage : failMessage,
                EvaluatedAt = DateTime.UtcNow
            };
        }
    }
}
