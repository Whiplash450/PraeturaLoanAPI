using Microsoft.EntityFrameworkCore;
using PraeturaLoanAPI.Models;

namespace PraeturaLoanAPI.Data
{
    public class LoanDbContext : DbContext
    {
        public LoanDbContext(DbContextOptions<LoanDbContext> options) : base(options) { }

        public DbSet<LoanApplication> LoanApplications { get; set; }
        public DbSet<DecisionLogEntry> DecisionLogs { get; set; }
    }
}
