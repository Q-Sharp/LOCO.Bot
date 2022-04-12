using LOCO.Bot.Shared.Entities;

namespace LOCO.Bot.Shared.Services
{
    public interface IAdminService
    {
        Task Restart(ulong? guildId = null, ulong? channelId = null);
        Task<Restart> ConsumeRestart();
    }
}