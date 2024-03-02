using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PopugJira.Auth.Models;

namespace PopugJira.Auth.Db;

public class PopugContext(DbContextOptions<PopugContext> options) : IdentityDbContext<Popug>(options)
{
}