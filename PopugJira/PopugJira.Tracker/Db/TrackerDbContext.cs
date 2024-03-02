using Microsoft.EntityFrameworkCore;

namespace PopugJira.Tracker.Db;

public class TrackerDbContext(DbContextOptions<TrackerDbContext> options) : DbContext(options)
{
    
}