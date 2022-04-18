using Discord.Commands;

namespace LOCO.Bot.Discord.Attributes;
public class RequireBotOwnerAttribute : PreconditionAttribute
{
    public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
    {
        if (context.User.Id == 301764235887902727)
        {
            return Task.FromResult(PreconditionResult.FromSuccess());
        }
        else
        {
            return Task.FromResult(PreconditionResult.FromError("Not the bot owner!"));
        }
    }
}
