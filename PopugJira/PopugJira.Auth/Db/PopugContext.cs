using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PopugJira.Auth.Models;

namespace PopugJira.Auth.Db;

public class PopugContext : IdentityDbContext<Popug>
{
    public PopugContext(DbContextOptions<PopugContext> options) : base(options)
    {
        Database.Migrate();
    }
}