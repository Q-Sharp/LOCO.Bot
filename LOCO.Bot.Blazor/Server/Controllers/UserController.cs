using AspNet.Security.OAuth.Discord;

using LOCO.Bot.Blazor.Shared.Auth;
using LOCO.Bot.Blazor.Shared.Defaults;
using LOCO.Bot.Data;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;

namespace LOCO.Bot.Blazor.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly Context _ctx;
    private readonly IConfiguration _configuration;

    public UserController(IConfiguration configuration, Context ctx)
    {
        _ctx = ctx;
        _configuration = configuration;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult GetCurrentUser()
        => Ok(User.Identity.IsAuthenticated ? CreateUserInfo(User) : DCUser.Anonymous);

    private IDCUser CreateUserInfo(ClaimsPrincipal claimsPrincipal)
    {
        if (!claimsPrincipal.Identity.IsAuthenticated)
            return DCUser.Anonymous;

        var dcUser = new DCUser
        {
            IsAuthenticated = true
        };

        var claimsIdentity = claimsPrincipal?.Identity as ClaimsIdentity;

        if (claimsIdentity is not null)
        {
            dcUser.NameClaimType = claimsIdentity.NameClaimType;
            dcUser.RoleClaimType = claimsIdentity.RoleClaimType;
        }
        else
        {
            dcUser.NameClaimType = ClaimTypes.Name;
            dcUser.RoleClaimType = ClaimTypes.Role;
        }

        if (claimsPrincipal.Claims.Any())
        {
            var allClaims = claimsPrincipal.Claims
                .Select(x => new ClaimValue(x.Type, x.Value))
                .ToList();

            dcUser.Claims = allClaims;
        }

        

        dcUser.Name = claimsIdentity.Name;
        dcUser.Id = ulong.Parse(claimsIdentity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "0");
        var avaHash = claimsIdentity.Claims.FirstOrDefault(c => c.Type == "urn:discord:avatar:hash")?.Value;
        dcUser.AvatarUrl = @$"https://cdn.discordapp.com/avatars/{dcUser.Id}/{avaHash}.png";

        return dcUser;
    }

    [HttpGet(ApiAuthDefaults.LogIn)]
    public IActionResult Login(string returnUrl = null) => Challenge(new AuthenticationProperties
    {
        RedirectUri = !string.IsNullOrEmpty(returnUrl) ? returnUrl : "/",
    });

    [HttpGet(ApiAuthDefaults.LogOut)]
    public async Task<IActionResult> LogOut()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignOutAsync(DiscordAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }
}
