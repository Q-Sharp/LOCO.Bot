using Discord;
using Discord.Commands;
using Discord.WebSocket;

using LOCO.Bot.Data;
using LOCO.Bot.Discord.Services.CommandHandler;
using LOCO.Bot.Discord.Services.Settings;
using LOCO.Bot.Shared.Services.Interfaces;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

namespace LOCO.Bot.Discord;

public static class DiscordSocketHost
{
    public static IHostBuilder CreateDiscordSocketHost(string[] args) =>
       Host.CreateDefaultBuilder(args)
           .UseSystemd()
           .ConfigureHostConfiguration(hostConfig =>
           {
               hostConfig.AddEnvironmentVariables(prefix: "LOCO_");
               hostConfig.AddCommandLine(args);
           })
           .UseDefaultServiceProvider(sp => sp.ValidateOnBuild = true)
           .UseSerilog((h, l) => l.ReadFrom.Configuration(h.Configuration))
           .ConfigureServices((h, s) =>
           {
               var config = h.Configuration;
               var discordConfig = config.GetSection("Discord:Settings");

               var sp =
               s.AddDbContext<IContext, Context>(o => o.UseNpgsql(config.GetConnectionString("Context")))
                .AddHostedService<DiscordWorker>()
                .AddSingleton(o => new DiscordSocketClient(new DiscordSocketConfig
                {
                    LogLevel = Enum.Parse<LogSeverity>(discordConfig.GetValue<string>("LogLevel"), true),
                    MessageCacheSize = discordConfig.GetValue<int>("MessageCacheSize")
                }))
                .AddSingleton(s => new CommandService(new CommandServiceConfig
                {
                    LogLevel = Enum.Parse<LogSeverity>(discordConfig["LogLevel"], true),
                    CaseSensitiveCommands = discordConfig.GetValue<bool>("CaseSensitiveCommands"),
                    DefaultRunMode = discordConfig.GetValue<RunMode>("DefaultRunMode"),
                    SeparatorChar = discordConfig.GetValue<string>("SeparatorChar").FirstOrDefault(),
                }))
                .AddSingleton<ICommandHandler, CommandHandler>()
                .AddSingleton<ISettingService, SettingService>()
                .BuildServiceProvider();
           });
}
