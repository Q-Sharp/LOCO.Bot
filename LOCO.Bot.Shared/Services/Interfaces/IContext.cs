using LOCO.Bot.Shared.Entities;

using Microsoft.EntityFrameworkCore;

namespace LOCO.Bot.Shared.Services.Interfaces;

public interface IContext
{
    DbSet<MemberGuess> MemberGuess { get; set; }
    DbSet<Setting> Setting { get; set; }

    Task MigrateAsync();
    Task TruncateAsync(string tableName);
    Task SaveChangesAsync();
}
