using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using PopugJira.Auth.Db;
using PopugJira.Auth.Models;

namespace PopugJira.Auth.DiExt;

public class InitAuthExtensions
{
    public static async Task InitRoles(IServiceProvider sp)
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

    public static async Task InitOAuthApps(IServiceProvider sp)
    {
        var context = sp.GetRequiredService<PopugContext>();
        await context.Database.EnsureCreatedAsync();

        var manager = sp.GetRequiredService<IOpenIddictApplicationManager>();

        if (await manager.FindByClientIdAsync("tracker") == null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "tracker",
                ClientSecret = "901564A5-E7FE-42CB-B10D-61EF6A8F3654",
                ConsentType = OpenIddictConstants.ConsentTypes.Implicit,
                DisplayName = "Popug Tracker",
                RedirectUris =
                {
                    new Uri("https://localhost:5142/callback/login/local")
                },
                PostLogoutRedirectUris =
                {
                    new Uri("https://localhost:5142/callback/logout/local")
                },
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.Endpoints.Logout,
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.ResponseTypes.Code,
                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Scopes.Roles
                },
                Requirements =
                {
                    OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange
                }
            });
        }
    }
}