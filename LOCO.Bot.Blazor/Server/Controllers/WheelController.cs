using LOCO.Bot.Data;

using Microsoft.AspNetCore.Mvc;

namespace LOCO.Bot.Blazor.Server.Controllers;

public class WheelController : ApiControllerBase<WheelController>
{
    private readonly Context _ctx;

    public WheelController(Context ctx, ILogger<WheelController> logger)
        : base(logger) => _ctx = ctx;

    [HttpGet]
    public IActionResult GetEntries()
    {
        try
        {
            var entries = _ctx.WheelEntry.ToArray();

            return Ok(entries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetEntries Exception", ex);
        }

        return BadRequest();
    }
}
