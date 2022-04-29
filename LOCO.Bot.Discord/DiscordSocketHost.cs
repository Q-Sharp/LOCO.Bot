using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;

using LOCO.Bot.Data;
using LOCO.Bot.Discord.Services;
using LOCO.Bot.Shared.Discord.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

namespace LOCO.Bot.Discord;

public static class DiscordSocketHost
{
    public static IHostBuilder CreateDiscordSocketHost(string[] args) => Host.CreateDefaultBuilder(args)
        .UseSystemd()
        .ConfigureHostOptions(options => options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore)
        .ConfigureAppConfiguration(appConfig => appConfig.AddUserSecrets(typeof(DiscordSocketHost).Assembly))
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

            s.AddDbContext<IContext, Context>(o => o.UseNpgsql(config.GetConnectionString("Context")))
             .AddHostedService<DiscordWorker>()
             .AddSingleton(o => new DiscordSocketClient(new DiscordSocketConfig
             {
                 LogLevel = Enum.Parse<LogSeverity>(discordConfig.GetValue<string>("LogLevel"), true),
                 MessageCacheSize = discordConfig.GetValue<int>("MessageCacheSize"),
                 AlwaysDownloadUsers = true
             }))
             .AddSingleton(o => new DiscordRestClient())
             .AddSingleton(s => new CommandService(new CommandServiceConfig
             {
                 LogLevel = discordConfig.GetValue<LogSeverity>("LogLevel"),
                 CaseSensitiveCommands = discordConfig.GetValue<bool>("CaseSensitiveCommands"),
                 DefaultRunMode = discordConfig.GetValue<RunMode>("DefaultRunMode"),
                 SeparatorChar = discordConfig.GetValue<string>("SeparatorChar").FirstOrDefault()
             }))
             .AddSingleton<ICommandHandler, CommandHandler>()
             .AddSingleton<ISettingService, SettingService>()
             .AddSingleton<IAdminService, AdminService>()
             .BuildServiceProvider();
    });
}
