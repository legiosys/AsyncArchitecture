using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using PopugJira.Auth.Models;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace PopugJira.Auth.Controllers;

[Route("api")]
public class ResourceController : Controller
{
    private readonly UserManager<Popug> _userManager;

    public ResourceController(UserManager<Popug> userManager)
        => _userManager = userManager;

    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("message")]
    public async Task<IActionResult> GetMessage()
    {
        var user = await _userManager.FindByIdAsync(User.GetClaim(Claims.Subject));
        if (user is null)
        {
            return Challenge(
                authenticationSchemes: OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictValidationAspNetCoreConstants.Properties.Error] = Errors.InvalidToken,
                    [OpenIddictValidationAspNetCoreConstants.Properties.ErrorDescription] =
                        "The specified access token is bound to an account that no longer exists."
                }));
        }

        return Content($"{user.UserName} has been successfully authenticated.");
    }
}
