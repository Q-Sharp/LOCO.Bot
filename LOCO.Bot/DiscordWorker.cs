using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;

using LOCO.Bot.Data;
using LOCO.Bot.Shared.Discord.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LOCO.Bot;

public class DiscordWorker : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<DiscordWorker> _logger;
    private readonly IConfiguration _config;
    private readonly IHostEnvironment _env;

    public DiscordWorker(IServiceProvider sp, ILogger<DiscordWorker> logger, IConfiguration config, IHostEnvironment env)
    {
        _sp = sp;
        _logger = logger;
        _config = config;
        _env = env;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTime.UtcNow);
            await InitAsync();
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }

    public override async Task StartAsync(CancellationToken cancellationToken) => await base.StartAsync(cancellationToken);

    public override async Task StopAsync(CancellationToken cancellationToken) => await Task.Run(() => Dispose(), cancellationToken);

    public async Task InitAsync()
    {
        var ctx = _sp.GetRequiredService<Context>();

        try
        {
            await ctx.MigrateAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Migration failed: {e}", e.Message);
            return;
        }

        var client = _sp.GetRequiredService<DiscordSocketClient>();
        _sp.GetRequiredService<CommandService>().Log += LogAsync;
        client.Log += LogAsync;

        var token = _env.IsDevelopment() ? _config["Discord:DevToken"] : _config["Discord:Token"];

        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();

        var rest = _sp.GetRequiredService<DiscordRestClient>();
        rest.Log += LogAsync;
        await rest.LoginAsync(TokenType.Bot, token);

        var ch = _sp.GetRequiredService<ICommandHandler>();
        await ch?.InitializeAsync();
    }

    public Task LogAsync(LogMessage message)
    {
        switch (message.Severity)
        {
            case LogSeverity.Critical:
            case LogSeverity.Error:
                _logger.LogError("{message}", message.Message);
                break;

            case LogSeverity.Warning:
                _logger.LogWarning("{message}", message.Message);
                break;

            case LogSeverity.Info:
                _logger.LogInformation("{message}", message.Message);
                break;

            case LogSeverity.Verbose:
            case LogSeverity.Debug:
                _logger.LogDebug("{message}", message.Message);
                break;
        }

        return Task.CompletedTask;
    }
}
