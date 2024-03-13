using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using PopugJira.Auth.Contracts;
using PopugJira.Auth.Db;
using PopugJira.Auth.Models;

namespace PopugJira.Auth.DiExt;

public class InitAuthExtensions
{
    public static async Task InitRoles(IServiceProvider sp)
    {
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        await AddRoleIfNeed(PopugPositionsEnum.Admin);
        await AddRoleIfNeed(PopugPositionsEnum.Accountant);
        await AddRoleIfNeed(PopugPositionsEnum.Manager);
        await AddRoleIfNeed(PopugPositionsEnum.Worker);
        
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

        await AddIfNeed(manager, "tracker", "901564A5-E7FE-42CB-B10D-61EF6A8F3654", 5142);
        await AddIfNeed(manager, "accounting", "AAA9EF13-A963-4149-B50D-B20E95BC6AC3", 5248);
        await AddIfNeed(manager, "analytics", "061E8469-C450-4E9F-81D2-07D482AA20A6", 7222);
    }

    private static async Task AddIfNeed(IOpenIddictApplicationManager manager,
        string clientId, string clientSecret, int port)
    {
        if (await manager.FindByClientIdAsync(clientId) == null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                ConsentType = OpenIddictConstants.ConsentTypes.Implicit,
                DisplayName = $"Popug {clientId}",
                RedirectUris =
                {
                    new Uri($"https://localhost:{port}/callback/login/local")
                },
                PostLogoutRedirectUris =
                {
                    new Uri($"https://localhost:{port}/callback/logout/local")
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