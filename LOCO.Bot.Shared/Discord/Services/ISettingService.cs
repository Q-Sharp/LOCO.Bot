using LOCO.Bot.Shared.Data.Entities;

namespace LOCO.Bot.Shared.Discord.Services;

public interface ISettingService
{
    Task<Setting> GetSettings(ulong guildId);
    Task SaveChangesAsync();
}
