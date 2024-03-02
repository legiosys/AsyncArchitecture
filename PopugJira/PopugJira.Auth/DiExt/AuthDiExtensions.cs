using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PopugJira.Auth.Db;
using PopugJira.Auth.Models;
using Quartz;
using Velusia.Server;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace PopugJira.Auth.DiExt;

public static class AuthDiExtensions
{
    public static void AddAuth(this IServiceCollection services)
    {
        AddQuartz(services);
        services.AddDbContext<PopugContext>(
            options =>
            {
                options.UseInMemoryDatabase("Users");
                options.UseOpenIddict();
            });
        
        AddOpenIdDict(services);
        
        services.AddIdentity<Popug,IdentityRole>()
            .AddEntityFrameworkStores<PopugContext>()
            .AddDefaultTokenProviders()
            .AddDefaultUI();
        
        services.Configure<IdentityOptions>(opt =>
        {
            opt.Password.RequireNonAlphanumeric = false;
            opt.Password.RequireDigit = false;
            opt.Password.RequireLowercase = false;
            opt.Password.RequireUppercase = false;
            opt.Password.RequiredLength = 3;
        });
    }

    private static void AddOpenIdDict(IServiceCollection services)
    {
        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<PopugContext>();

                options.UseQuartz();
            })
            
            /*.AddClient(options =>
            {
                options.AllowAuthorizationCodeFlow();

                // Register the signing and encryption credentials used to protect
                // sensitive data like the state tokens produced by OpenIddict.
                options.AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate();

                // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
                options.UseAspNetCore()
                    .EnableStatusCodePagesIntegration()
                    .EnableRedirectionEndpointPassthrough();

                // Register the System.Net.Http integration and use the identity of the current
                // assembly as a more specific user agent, which can be useful when dealing with
                // providers that use the user agent as a way to throttle requests (e.g Reddit).
                options.UseSystemNetHttp()
                    .SetProductInformation(typeof(Program).Assembly);
                
                options.UseWebProviders()
                    .AddGitHub(options =>
                {
                    options.SetClientId("c4ade52327b01ddacff3")
                        .SetClientSecret("da6bed851b75e317bf6b2cb67013679d9467c122")
                        .SetRedirectUri("callback/login/github");
                });;
            })*/

            // Register the OpenIddict server components.
            .AddServer(options =>
            {
                // Enable the authorization, logout, token and userinfo endpoints.
                options.SetAuthorizationEndpointUris("connect/authorize")
                    .SetLogoutEndpointUris("connect/logout")
                    .SetTokenEndpointUris("connect/token")
                    .SetUserinfoEndpointUris("connect/userinfo");

                // Mark the "email", "profile" and "roles" scopes as supported scopes.
                options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles);

                // Note: this sample only uses the authorization code flow but you can enable
                // the other flows if you need to support implicit, password or client credentials.
                options.AllowAuthorizationCodeFlow();

                // Register the signing and encryption credentials.
                options.AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate();

                // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
                options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableLogoutEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableUserinfoEndpointPassthrough()
                    .EnableStatusCodePagesIntegration();
            })

            // Register the OpenIddict validation components.
            .AddValidation(options =>
            {
                // Import the configuration from the local OpenIddict server instance.
                options.UseLocalServer();

                // Register the ASP.NET Core host.
                options.UseAspNetCore();
            });
    }

    private static void AddQuartz(IServiceCollection services)
    {
        services.AddQuartz(options =>
        {
            options.UseMicrosoftDependencyInjectionJobFactory();
            options.UseSimpleTypeLoader();
            options.UseInMemoryStore();
        });
        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);
    }

    /*services.AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddIdentityCookies();
        services.ConfigureApplicationCookie(opt =>
        {
            opt.Cookie.Name = "Auth";

            opt.LoginPath = "/login";
            opt.SlidingExpiration = true;
            opt.Events.OnRedirectToLogin = context =>
            {
                if (context.Request.Path.StartsWithSegments(new PathString("/api")))
                    context.Response.StatusCode = 401;
                else
                    context.Response.Redirect(context.RedirectUri);

                return Task.CompletedTask;
            };
            opt.Events.OnRedirectToAccessDenied = context =>
            {
                if (context.Request.Path.StartsWithSegments(new PathString("/api")) ||
                    context.Request.Path.StartsWithSegments(new PathString("/dist")))
                {
                    context.Response.StatusCode = 403;
                }
                else
                {
                    context.Response.Redirect(context.RedirectUri);
                }

                return Task.CompletedTask;
            };
        });
        services.AddAuthorizationBuilder()
            .AddDefaultPolicy("default", policyBuilder => policyBuilder.RequireAuthenticatedUser());*/
}