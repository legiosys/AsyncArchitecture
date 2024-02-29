using Microsoft.AspNetCore.Identity;

namespace PopugJira.Auth.Models;

public class PopugPosition : IdentityRole
{
    public PopugPosition(string position) : base(position)
    {
    }
}