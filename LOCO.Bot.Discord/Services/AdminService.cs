using LOCO.Bot.Shared.Data.Entities;
using LOCO.Bot.Shared.Discord.Services;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LOCO.Bot.Discord.Services;

public class AdminService : LOCOBotService<AdminService>, IAdminService
{
    private readonly IContext _ctx;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    public AdminService(IContext context, IHostApplicationLifetime hostApplicationLifetime, ILogger<AdminService> logger)
        : base(logger)
    {
        _ctx = context;
        _hostApplicationLifetime = hostApplicationLifetime;
    }

    public async Task<Restart> ConsumeRestart()
    {
        try
        {
            Restart r = _ctx.Restart.AsEnumerable().OrderBy(r => r.Id).FirstOrDefault();

            if (r != null)
            {
                _ctx.Restart.Remove(r);
                await _ctx.SaveChangesAsync();
                return r;
            }
            else
            {
                return default;
            }
        }
        catch
        {
            return default;
        }
    }

    public async Task Restart(ulong? guildId = null, ulong? channelId = null)
    {
        if (guildId.HasValue && channelId.HasValue)
        {
            var r = _ctx.Restart.Add(new Restart()).Entity;
            r.Guild = guildId.Value;
            r.Channel = channelId.Value;
            await _ctx?.SaveChangesAsync();
        }

        _hostApplicationLifetime?.StopApplication();
    }
}
