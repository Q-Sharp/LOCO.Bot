using LOCO.Bot.Shared.Data.Entities;
using LOCO.Bot.Shared.Discord.Services;

using Microsoft.EntityFrameworkCore;

namespace LOCO.Bot.Data;

public class Context : DbContext, IContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
        {
            options.UseNpgsql($@"Server=127.0.0.1;Port=5433;Database=LOCOBotDB;Username=postgres;Password=P0stGresSQL2021");
        }
    }

    public Context()
    {
    }

    public Context(DbContextOptions<Context> options = null) : base(options)
    {
    }

    public DbSet<Guess> Guess { get; set; }
    public DbSet<Setting> Setting { get; set; }
    public DbSet<Restart> Restart { get; set; }

    public DbSet<WheelEntry> WheelEntry { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.ApplyConfigurationsFromAssembly(typeof(Context).Assembly)
                    .UseIdentityByDefaultColumns();

    public async Task MigrateAsync() => await Database.MigrateAsync();

    public async Task SaveChangesAsync() => await base.SaveChangesAsync();
}
