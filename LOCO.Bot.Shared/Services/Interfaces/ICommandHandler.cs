using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace LOCO.Bot.Shared.Services.Interfaces;

public interface ICommandHandler
{
    Task Client_Disconnected(Exception arg);
    Task Client_HandleCommandAsync(SocketMessage arg);
    Task Client_Ready();
    Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result);
    Task InitializeAsync();
    Task LogAsync(LogMessage logMessage);
}
