using Microsoft.AspNetCore.Identity;

namespace PopugJira.Auth.Models;

public class Popug : IdentityUser
{
    public Guid PopugId() => Guid.Parse(Id);
}