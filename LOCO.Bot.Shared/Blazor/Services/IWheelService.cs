using LOCO.Bot.Shared.Data.Entities;

namespace LOCO.Bot.Shared.Blazor.Services
{
    public interface IWheelService
    {
        Task<ICollection<WheelEntry>> GetWheelEntriesShuffledAsync();
    }
}