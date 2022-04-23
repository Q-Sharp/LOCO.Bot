using LOCO.Bot.Shared.Data.Entities;

namespace LOCO.Bot.Shared.Web.Services
{
    public interface IWheelService
    {
        Task<IEnumerable<WheelEntry>> GetWheelEntriesShuffledAsync();
        Task<IEnumerable<WheelEntry>> GetNextWheelEntries(int qty);
    }
}