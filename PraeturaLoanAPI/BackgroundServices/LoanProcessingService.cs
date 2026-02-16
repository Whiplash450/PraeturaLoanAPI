using Microsoft.EntityFrameworkCore;
using PraeturaLoanAPI.Data;
using PraeturaLoanAPI.Services;

namespace PraeturaLoanAPI.BackgroundServices
{
    public class LoanProcessingService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public LoanProcessingService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<LoanDbContext>();
                        var eligibilityService = scope.ServiceProvider.GetRequiredService<IEligibilityService>();

                        //Pull all pending applications
                        var pendingApplications = await context.LoanApplications.Where(a => a.Status == "Pending").ToListAsync(stoppingToken);

                        //Evaluate all pending applications
                        foreach (var application in pendingApplications)
                        {
                            var results = eligibilityService.Evaluate(application);
                            if (results == null || !results.Any())
                            {
                                continue; //continue to skip broken/null application but process rest of queue -- ideally log the skip here also
                            }
                            //
                            context.DecisionLogs.AddRange(results);
                            //
                            application.Status = results.All(r => r.Passed) ? "Approved" : "Rejected";
                            application.ReviewedAt = DateTime.UtcNow;
                        }

                        await context.SaveChangesAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    // Log error but don't crash the service
                }

                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }
        }
    }
}
