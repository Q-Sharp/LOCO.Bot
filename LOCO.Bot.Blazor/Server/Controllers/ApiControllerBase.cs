
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LOCO.Bot.Blazor.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[ValidateAntiForgeryToken]
[Authorize]
public class ApiControllerBase<TController> : ControllerBase
{
    protected ILogger<TController> _logger { get; }
    public ApiControllerBase(ILogger<TController> logger) => _logger = logger;
}
