using LOCO.Bot.Data;
using LOCO.Bot.Shared.Entities;
using LOCO.Bot.Shared.Services.Interfaces;

using Microsoft.Extensions.Logging;

namespace LOCO.Bot.Discord.Services.Settings;

public class SettingService : LOCOBotService<SettingService>, ISettingService
{
    private readonly Context _ctx;
    private readonly Setting _setting;

    public SettingService(ILogger<SettingService> logger, Context ctx) : base(logger)
    {
        _ctx = ctx;
        _setting = ctx.Setting.FirstOrDefault();

        if (_setting is null)
        {
            _setting = ctx.Add(new Setting()).Entity;
            ctx.SaveChanges();
        }
    }

    public Setting Settings => _setting;

    public async Task SaveChangesAsync() => await _ctx.SaveChangesAsync();
}
