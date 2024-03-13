using Microsoft.EntityFrameworkCore;
using PopugJira.Accounting.Models;

namespace PopugJira.Accounting.Db;

public class AccountingDbContext : DbContext
{
    public DbSet<Popug> Popugs { get; set; }
    public DbSet<PopugTask> Tasks { get; set; }
    public DbSet<HandleError> HandleErrors { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<BillingCycle> BillingCycles { get; set; }
    
    public AccountingDbContext(DbContextOptions<AccountingDbContext> options) : base(options)
    {
        Database.Migrate();
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}