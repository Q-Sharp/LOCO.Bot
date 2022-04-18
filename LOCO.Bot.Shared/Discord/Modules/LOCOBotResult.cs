using Discord;
using Discord.Commands;

namespace LOCO.Bot.Shared.Discord.Modules;

public class LOCOBotResult : RuntimeResult
{
    private LOCOBotResult(CommandError? error, string reason = null, IMessage answer = null) : base(error, reason) => AnswerSent = answer;

    public IMessage AnswerSent { get; set; }

    public static LOCOBotResult Create(CommandError? error, string reason = null, IMessage answer = null) => new(error, reason, answer);
}
