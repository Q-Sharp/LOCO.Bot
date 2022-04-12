using Discord;
using Discord.Commands;

namespace LOCO.Bot.Shared.Modules;

public interface IMemberGuessModule
{
    Task<RuntimeResult> Publish([Remainder] string endResult);
    Task<RuntimeResult> Guess([Remainder] string guess);
    Task<RuntimeResult> SetChannel(IChannel guessChannel);
    Task<RuntimeResult> SetRole(IRole guessRole);
    Task<RuntimeResult> Start();
    Task<RuntimeResult> Stop();
}
