using LOCO.Bot.Shared.Entities;

namespace LOCO.Bot.Shared.Services;

public interface ISettingService
{
    Task<Setting> GetSettings(ulong guildId);
    Task SaveChangesAsync();
}
