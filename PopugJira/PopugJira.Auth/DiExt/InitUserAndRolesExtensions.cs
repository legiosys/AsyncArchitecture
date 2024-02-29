using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using PopugJira.Auth.Models;

namespace PopugJira.Auth.DiExt;

public class InitUserAndRolesExtensions
{
    public static async Task InitPopugDb(IServiceProvider sp)
    {
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        await AddRoleIfNeed("admin");
        await AddRoleIfNeed("worker");
        await AddRoleIfNeed("manager");
        await AddRoleIfNeed("accountant");

        var userManager = sp.GetRequiredService<UserManager<Popug>>();
        var popug = await userManager.FindByNameAsync("admin");
        if (popug == null)
        {
            popug = new Popug() {UserName = "admin"};
            await userManager.CreateAsync(popug, "admin");
        }

        if(!await userManager.IsInRoleAsync(popug,"admin"))
            await userManager.AddToRoleAsync(popug, "admin");

        async Task AddRoleIfNeed(string role)
        {
            if (await roleManager.RoleExistsAsync(role))
                return;

            var result = await roleManager.CreateAsync(new IdentityRole(role));
            if(!result.Succeeded)
                Console.WriteLine(JsonSerializer.Serialize(result));
        }
    }
}