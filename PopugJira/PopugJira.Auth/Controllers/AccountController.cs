using System.Collections.Immutable;
using Confluent.Kafka;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PopugJira.Auth.Contracts;
using PopugJira.Auth.Models;
using PopugJira.Auth.ViewModels.Account;

namespace PopugJira.Auth.Controllers;

public class AccountController : Controller
{
    // GET
    public IActionResult Login([FromQuery] string? returnUrl = null)
    {
        return View(new LoginViewModel()
        {
            Login = string.Empty,
            Password = string.Empty,
            Remember = true,
            ReturnUrl = returnUrl
        });
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromForm] LoginViewModel request,
        [FromServices] SignInManager<Popug> signInManager)
    {
        var result = await signInManager.PasswordSignInAsync(request.Login, request.Password, request.Remember, false);
        if (result.Succeeded)
            return request.ReturnUrl != null 
                ? Redirect(request.ReturnUrl) 
                : RedirectToAction("Index", "Home");

        return View(request with {Error = "Wrong popug!"});
    }
    

    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromForm]RegisterViewModel request, 
        [FromServices] UserManager<Popug> userManager, 
        [FromServices] ITopicProducer<PopugChanged> popugChangedProducer)
    {
        var popug = new Popug() {UserName = request.Login};
        var result = await userManager.CreateAsync(popug, request.Password);
        if (!result.Succeeded)
        {
            request.Error = result.Errors.FirstOrDefault()?.Description;
            return View(request);
        }
        
        result = await userManager.AddToRoleAsync(popug, request.Position);
        if (!result.Succeeded)
        {
            request.Error = result.Errors.FirstOrDefault()?.Description;
            return View(request);
        }

        await popugChangedProducer.Produce(new PopugChanged(popug.PopugId(), PopugChangedTypes.Created,
            new Dictionary<string, string>
                {
                    {"position", request.Position},
                    {"login", request.Login}
                }));
        
        return RedirectToAction("Login");
    }
}