using AtlasX.Engine.Extensions;
using AtlasX.Web.Service.OAuth.Repositories.Interfaces;
using AtlasX.Web.Service.OAuth.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AtlasX.Web.Service.Controllers;

[Route("[controller]")]
public class AppLoginController : Controller
{
    private readonly IUserInfoRepository _userInfoRepository;
    private readonly ILogger<AppLoginController> _logger;

    public AppLoginController(ILogger<AppLoginController> logger, IUserInfoRepository userInfoRepository)
    {
        _userInfoRepository = userInfoRepository;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public IActionResult Index()
    {
        if (User.Identity is { IsAuthenticated: true })
        {
            return RedirectToAction("Welcome");
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(LoginViewModel model, string returnUrl)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userInfo = _userInfoRepository.Get(model.Username, model.Password, null);
        if (userInfo == null)
        {
            ModelState.AddModelError("Password", "The username or password is incorrect.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userInfo.Id.ToString())
        };

        var claimsIdentity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            //AllowRefresh = <bool>,
            // Refreshing the authentication session should be allowed.

            //ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
            // The time at which the authentication ticket expires. A 
            // value set here overrides the ExpireTimeSpan option of 
            // CookieAuthenticationOptions set with AddCookie.

            //IsPersistent = true,
            // Whether the authentication session is persisted across 
            // multiple requests. When used with cookies, controls
            // whether the cookie's lifetime is absolute (matching the
            // lifetime of the authentication ticket) or session-based.

            //IssuedUtc = <DateTimeOffset>,
            // The time at which the authentication ticket was issued.

            //RedirectUri = <string>
            // The full path or absolute URI to be used as an http 
            // redirect response value.
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);


        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            return RedirectToAction("Welcome");
        }

        return Redirect(returnUrl);
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index");
    }

    [HttpGet("welcome")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public IActionResult Welcome()
    {
        var userInfo = _userInfoRepository.Get(User.GetUserId());
        return View(userInfo);
    }
}