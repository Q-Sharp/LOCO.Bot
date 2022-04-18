using LOCO.Bot.Shared.Data.Entities;

using Microsoft.EntityFrameworkCore;

namespace LOCO.Bot.Shared.Discord.Services;

public interface IContext
{
    DbSet<Guess> Guess { get; set; }
    DbSet<Setting> Setting { get; set; }
    DbSet<Restart> Restart { get; set; }

    Task MigrateAsync();
    Task SaveChangesAsync();
}
