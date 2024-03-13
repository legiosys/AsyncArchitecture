using Microsoft.AspNetCore.Authentication.Cookies;
using OpenIddict.Client;
using PopugJira.Analytics.Db;
using PopugJira.Auth.Contracts;
using Quartz;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace PopugJira.Analytics.DiExt;

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

        services.AddAuthorization(options =>
        {
            options.AddPolicy("admin", policy => policy.RequireClaim(Claims.Role, PopugPositionsEnum.Admin));
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
                       .UseDbContext<AnalyticsDbContext>();
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

                    ClientId = "analytics",
                    ClientSecret = "061E8469-C450-4E9F-81D2-07D482AA20A6",
                    Scopes = { Scopes.Roles},
                    RedirectUri = new Uri("callback/login/local", UriKind.Relative),
                    PostLogoutRedirectUri = new Uri("callback/logout/local", UriKind.Relative)
                });
            });

        services.AddHttpClient();
    }
}