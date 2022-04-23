using Discord;
using Discord.Commands;
using Discord.WebSocket;

using LOCO.Bot.Data;
using LOCO.Bot.Discord.Helpers;
using LOCO.Bot.Shared.Discord.Modules;
using LOCO.Bot.Shared.Discord.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System.Diagnostics;

namespace LOCO.Bot.Discord.Services;

public partial class CommandHandler : ICommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly IServiceProvider _services;
    private readonly ILogger<CommandHandler> _logger;
    private readonly IConfiguration _config;

    public CommandHandler(IServiceProvider services, CommandService commands,
        DiscordSocketClient client, ILogger<CommandHandler> logger, IConfiguration config, Context ctx)
    {
        _commands = commands;
        _client = client;
        _logger = logger;
        _config = config;
        _services = services;
    }

    public async Task InitializeAsync()
    {
        await _commands.AddModulesAsync(GetType().Assembly, _services);

        _commands.CommandExecuted += CommandExecutedAsync;
        _commands.Log += LogAsync;

        _client.MessageReceived += Client_HandleCommandAsync;
        _client.Ready += Client_Ready;
        _client.Log += LogAsync;
        _client.Disconnected += Client_Disconnected;
    }

    public Task LogAsync(LogMessage logMessage)
    {
        if (logMessage.Exception is CommandException cmdException)
        {
            _logger.LogError("'{User}' failed to execute '{Name}' in '{Channel}'.",
                cmdException.Context.User, cmdException.Command.Name, cmdException.Context.Channel);
        }

        if(logMessage.Exception is not null)
            _logger.LogError("{Exception}", logMessage.Exception);

        return Task.CompletedTask;
    }

    public async Task Client_Ready()
    {
        _logger.Log(LogLevel.Information, "Bot is connected!");

        // set status
        await _client.SetGameAsync(_config["Discord:Activity"]);

        // handle restart information
        var adminService = _services.GetService<IAdminService>();
        var r = await adminService.ConsumeRestart();
        if (r is not null)
        {
            var dest = _client.GetGuild(r.Guild).GetTextChannel(r.Channel);
            await dest.SendMessageAsync("Bot service has been restarted!");
        }

        _logger.LogInformation("Bot is online!");
    }

    public async Task Client_HandleCommandAsync(SocketMessage arg)
    {
        if (arg is not SocketUserMessage msg)
        {
            return;
        }

        var context = new SocketCommandContext(_client, msg);

        if (msg.Author.Id == _client.CurrentUser.Id || msg.Author.IsBot)
        {
            return;
        }

        var pos = 0;
        if (msg.HasStringPrefix(_config["Discord:Prefix"], ref pos, StringComparison.OrdinalIgnoreCase)
            || msg.HasMentionPrefix(_client.CurrentUser, ref pos))
        {
            await _commands.ExecuteAsync(context, pos, _services);
        }
    }

    public async Task Client_Disconnected(Exception arg)
    {
        _logger.LogError("{Message}", arg.Message);

        await Task.Run(() =>
        {
            var locoBot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.FriendlyName);
            Process.Start(locoBot);
            Environment.Exit(0);
        });
    }

    public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
    {
        if (!command.IsSpecified)
        {
            await context.Channel.SendMessageAsync($"I don't know this command: {context.Message}");
            return;
        }

        if (result is LOCOBotResult runTimeResult)
        {
            if (result.IsSuccess)
            {
                if (runTimeResult.Reason is not null)
                {
                    _ = await context.Channel.SendMessageAsync(runTimeResult.Reason);
                }

                return;
            }

            await context.Channel.SendMessageAsync($"{runTimeResult.Error}: {runTimeResult.Reason}");

            var member = context.User.GetUserAndDiscriminator();
            var moduleName = command.Value.Module.Name;
            var commandName = command.Value.Name;

            _logger.LogError("{member} tried to use {commandName} (module: {moduleName}) this resulted in a {Error}",
                member, commandName, moduleName, runTimeResult.Error);
        }
    }

    private static async Task DeleteMessage(IMessage userMsg, IMessage answer)
    {
        await Task.Delay(TimeSpan.FromMinutes(2));

        if (userMsg is not null)
        {
            await userMsg.DeleteAsync(new RequestOptions { AuditLogReason = "Autoremoved" });
        }

        if (answer is not null)
        {
            await answer.DeleteAsync(new RequestOptions { AuditLogReason = "Autoremoved" });
        }
    }
}
