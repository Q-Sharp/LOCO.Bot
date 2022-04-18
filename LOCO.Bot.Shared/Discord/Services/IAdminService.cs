using LOCO.Bot.Shared.Data.Entities;

namespace LOCO.Bot.Shared.Discord.Services
{
    public interface IAdminService
    {
        Task Restart(ulong? guildId = null, ulong? channelId = null);
        Task<Restart> ConsumeRestart();
    }
}