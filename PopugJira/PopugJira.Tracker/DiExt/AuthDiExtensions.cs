using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Client;
using PopugJira.Tracker.Db;
using Quartz;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace PopugJira.Tracker.DiExt;

public static class AuthDiExtensions
{
    public static void AddAuth(this IServiceCollection services)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })

        .AddCookie(options =>
        {
            options.LoginPath = "/login";
            options.LogoutPath = "/logout";
            options.ExpireTimeSpan = TimeSpan.FromMinutes(50);
            options.SlidingExpiration = false;
        });
        services.AddQuartz(options =>
        {
            options.UseMicrosoftDependencyInjectionJobFactory();
            options.UseSimpleTypeLoader();
            options.UseInMemoryStore();
        });
        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                       .UseDbContext<TrackerDbContext>();
                options.UseQuartz();
            })
            .AddClient(options =>
            {
                options.AllowAuthorizationCodeFlow();

                options.AddDevelopmentEncryptionCertificate()
                       .AddDevelopmentSigningCertificate();
                
                options.UseAspNetCore()
                       .EnableStatusCodePagesIntegration()
                       .EnableRedirectionEndpointPassthrough()
                       .EnablePostLogoutRedirectionEndpointPassthrough();
                
                options.UseSystemNetHttp()
                       .SetProductInformation(typeof(Program).Assembly);
                
                options.AddRegistration(new OpenIddictClientRegistration
                {
                    Issuer = new Uri("https://localhost:5185/", UriKind.Absolute),

                    ClientId = "tracker",
                    ClientSecret = "901564A5-E7FE-42CB-B10D-61EF6A8F3654",
                    Scopes = { Scopes.Roles },
                    RedirectUri = new Uri("callback/login/local", UriKind.Relative),
                    PostLogoutRedirectUri = new Uri("callback/logout/local", UriKind.Relative)
                });
            });

        services.AddHttpClient();
    }
}