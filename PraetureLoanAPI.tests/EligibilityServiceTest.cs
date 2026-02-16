using PraeturaLoanAPI.Services;
using PraeturaLoanAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PraetureLoanAPI.tests
{
    public class EligibilityServiceTest
    {
        private readonly EligibilityService _service = new EligibilityService();
        //
        [Fact]
        public void Evaluate_ValidApplication_AllRulesPass()
        {
            var application = new LoanApplication
            {
                Id = Guid.NewGuid(),
                Name = "Bob Test",
                Email = "bob@test.com",
                MonthlyIncome = 3500,
                RequestedAmount = 8000,
                TermMonths = 36
            };
            //
            var results = _service.Evaluate(application);
            //
            Assert.Equal(3, results.Count);
            Assert.All(results, r => Assert.True(r.Passed));
        }
    }
}
