using Microsoft.EntityFrameworkCore;
using PopugJira.Tracker.Models;

namespace PopugJira.Tracker.Db;

public class TrackerDbContext : DbContext
{
    public DbSet<Popug> Popugs { get; set; }
    public DbSet<PopugTask> Tasks { get; set; }

    public TrackerDbContext(DbContextOptions<TrackerDbContext> options) : base(options)
    {
        Database.Migrate();
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}