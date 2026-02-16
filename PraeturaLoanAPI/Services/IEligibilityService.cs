using PraeturaLoanAPI.Models;

namespace PraeturaLoanAPI.Services
{
    public interface IEligibilityService
    {
        List<DecisionLogEntry> Evaluate(LoanApplication application);
    }
}
