using Microsoft.EntityFrameworkCore;
using PopugJira.Analytics.Models;

namespace PopugJira.Analytics.Db;

public class AnalyticsDbContext : DbContext
{
    public DbSet<PopugTask> PopugTasks { get; set; }
    public DbSet<TopsBalanceChange> TopsBalanceChanges { get; set; }
    public DbSet<PopugBalanceChange> PopugBalanceChanges { get; set; }
    
    public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options)
    {
        Database.Migrate();
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}