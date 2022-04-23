using LOCO.Bot.Data;
using LOCO.Bot.Shared.Data.Entities;

using Microsoft.AspNetCore.Mvc;

namespace LOCO.Bot.Web.Server.Controllers;

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

    [HttpPost]
    public IActionResult UpdateEntry(WheelEntry we)
    {
        try
        {
            var dbWE = _ctx.WheelEntry.FirstOrDefault(x => x.Id == we.Id);

            if (dbWE is null)
                _ctx.WheelEntry.Add(we);
            else
                dbWE.Update(we);

            _ctx.SaveChanges();
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateEntry Exception", ex);
        }

        return BadRequest();
    }

    [HttpDelete("delete/{id}")]
    public IActionResult Delete(int id)
    {
        try
        {
            var dbWE = _ctx.WheelEntry.FirstOrDefault(x => x.Id == id);

            if (dbWE is not null)
            {
                _ctx.Remove(dbWE);
                _ctx.SaveChanges();
                return Ok();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateEntry Exception", ex);
        }

        return BadRequest();
    }
}
