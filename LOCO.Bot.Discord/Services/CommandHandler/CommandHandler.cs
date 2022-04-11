using Discord;
using Discord.Commands;
using Discord.WebSocket;

using LOCO.Bot.Data;
using LOCO.Bot.Discord.Helpers;
using LOCO.Bot.Shared.Modules;
using LOCO.Bot.Shared.Services.Interfaces;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using System.Diagnostics;

namespace LOCO.Bot.Discord.Services.CommandHandler;

public partial class CommandHandler : ICommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly IServiceProvider _services;
    private readonly ILogger<CommandHandler> _logger;
    private readonly IConfiguration _config;
    private readonly Context _ctx;

    public CommandHandler(IServiceProvider services, CommandService commands,
        DiscordSocketClient client, ILogger<CommandHandler> logger, IConfiguration config, Context ctx)
    {
        _commands = commands;
        _client = client;
        _logger = logger;
        _config = config;
        _services = services;
        _ctx = ctx;
    }

    public async Task InitializeAsync()
    {
        await _commands.AddModulesAsync(GetType().Assembly, _services);

        _commands.CommandExecuted += CommandExecutedAsync;
        _commands.Log += LogAsync;

        _client.MessageReceived += Client_HandleCommandAsync;
        _client.ReactionAdded += Client_ReactionAdded; ;
        _client.Ready += Client_Ready;
        _client.Log += LogAsync;
        _client.Disconnected += Client_Disconnected;
    }

    private Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction arg3)
    {
        throw new NotImplementedException();
    }

    public async Task LogAsync(LogMessage logMessage)
    {
        if (logMessage.Exception is CommandException cmdException)
        {
            await cmdException.Context.Channel.SendMessageAsync("Something went catastrophically wrong!");
            _logger.LogError("{cmdException.Context.User} failed to execute '{cmdException.Command.Name}' in {cmdException.Context.Channel}.",
                cmdException.Context.User, cmdException.Command.Name, cmdException.Context.Channel);
        }
    }

    public async Task Client_Ready()
    {
        _logger.Log(LogLevel.Information, "Bot is connected!");

        // set status
        await _client.SetGameAsync($"LOCO Bot since 2022");
        _logger.LogInformation("Bot is online!");
    }

    public async Task Client_HandleCommandAsync(SocketMessage arg)
    {
        if (arg is not SocketUserMessage msg)
            return;

        var context = new SocketCommandContext(_client, msg);

        if (msg.Author.Id == _client.CurrentUser.Id || msg.Author.IsBot)
            return;

        var pos = 0;
        if (msg.HasStringPrefix(_config["Discord:Prefix"], ref pos, StringComparison.OrdinalIgnoreCase)
            || msg.HasMentionPrefix(_client.CurrentUser, ref pos))
        {
            await _commands.ExecuteAsync(context, pos, _services);
        }
    }

    public async Task Client_Disconnected(Exception arg)
    {
        _logger.LogError(arg.Message, arg);

        await Task.Run(() =>
        {
            var mmBot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.FriendlyName);
            Process.Start(mmBot);
            Environment.Exit(0);
        });
    }

    public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
    {
        // error happened
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
                    _ = await context.Channel.SendMessageAsync(runTimeResult.Reason);

                return;
            }

            await context.Channel.SendMessageAsync($"{runTimeResult.Error}: {runTimeResult.Reason}");

            var member = context.User.GetUserAndDiscriminator();
            var moduleName = command.Value.Module.Name;
            var commandName = command.Value.Name;

            _logger.LogError("{member} tried to use {commandName} (module: {moduleName}) this resulted in a {runTimeResult.Error}",
                member, commandName, moduleName, runTimeResult.Error);
        }
    }

    private async static Task DeleteMessage(IMessage userMsg, IMessage answer)
    {
        await Task.Delay(TimeSpan.FromMinutes(2));

        if (userMsg is not null)
            await userMsg.DeleteAsync(new RequestOptions { AuditLogReason = "Autoremoved" });

        if (answer is not null)
            await answer.DeleteAsync(new RequestOptions { AuditLogReason = "Autoremoved" });
    }
}
