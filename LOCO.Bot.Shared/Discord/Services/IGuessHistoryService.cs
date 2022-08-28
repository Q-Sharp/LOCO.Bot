namespace LOCO.Bot.Shared.Discord.Services
{
    public interface IGuessHistoryService
    {
        Task CreateHistory(ulong gid);
        Task UpdateHistoryEnd(ulong gid);
        Task UpdateHistoryResult(ulong gid);
    }
}