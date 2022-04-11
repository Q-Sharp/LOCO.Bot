using Discord;
using Discord.Commands;

using LOCO.Bot.Data;
using LOCO.Bot.Shared.Modules;
using LOCO.Bot.Shared.Services.Interfaces;

namespace LOCO.Bot.Discord.Modules;

public abstract class LOCOBotModule : ModuleBase
{
    protected readonly IContext _ctx;
    protected readonly ICommandHandler _commandHandler;
    protected readonly ISettingService _settingService;

    public LOCOBotModule(IContext ctx, ISettingService settingService, ICommandHandler commandHandler)
    {
        _ctx = ctx;
        _commandHandler = commandHandler;
        _settingService = settingService;
    }

    public static LOCOBotResult FromSuccess(string successMessage = null, IMessage answer = null)
        => LOCOBotResult.Create(null, successMessage, answer);
    public static LOCOBotResult FromError(CommandError error, string reason, IMessage answer = null)
        => LOCOBotResult.Create(error, reason, answer);
    public static LOCOBotResult FromErrorObjectNotFound(string objectname, string searchstring, IMessage answer = null)
        => LOCOBotResult.Create(CommandError.ObjectNotFound, $"{objectname}: {searchstring}", answer);
    public static LOCOBotResult FromErrorUnsuccessful(string error, IMessage answer = null)
        => LOCOBotResult.Create(CommandError.Unsuccessful, error, answer);
    public static LOCOBotResult FromIgnore()
        => LOCOBotResult.Create(null, null, null);
}
