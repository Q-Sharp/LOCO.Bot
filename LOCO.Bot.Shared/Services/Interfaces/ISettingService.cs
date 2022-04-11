using LOCO.Bot.Shared.Entities;

namespace LOCO.Bot.Shared.Services.Interfaces;

public interface ISettingService
{
    Setting Settings { get; }

    Task SaveChangesAsync();
}
