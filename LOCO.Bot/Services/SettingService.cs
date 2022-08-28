using LOCO.Bot.Data;
using LOCO.Bot.Shared.Data.Entities;
using LOCO.Bot.Shared.Discord.Services;

using Microsoft.Extensions.Logging;

namespace LOCO.Bot.Services;

public class SettingService : LOCOBotService<SettingService>, ISettingService
{
    private readonly Context _ctx;

    public SettingService(ILogger<SettingService> logger, Context ctx) : base(logger) => _ctx = ctx;

    public async Task<Setting> GetSettings(ulong guildId)
    {
        var setting = _ctx.Setting.FirstOrDefault(x => x.GuildId == guildId);

        if (setting is null)
        {
            setting = _ctx.Add(new Setting()).Entity;
            setting.GuildId = guildId;
            await _ctx.SaveChangesAsync();
        }

        return setting;
    }

    public async Task SaveChangesAsync() => await _ctx.SaveChangesAsync();
}
