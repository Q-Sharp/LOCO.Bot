using LOCO.Bot.Shared.Entities;

using Microsoft.EntityFrameworkCore;

namespace LOCO.Bot.Shared.Services;

public interface IContext
{
    DbSet<Guess> Guess { get; set; }
    DbSet<Setting> Setting { get; set; }
    DbSet<Restart> Restart { get; set; }

    Task MigrateAsync();
    Task TruncateAsync(string tableName);
    Task SaveChangesAsync();
}
