using Discord;
using Discord.Commands;

using LOCO.Bot.Shared.Discord.Modules;
using LOCO.Bot.Shared.Discord.Services;

using Microsoft.Extensions.Logging;

namespace LOCO.Bot.Discord.Modules;

public abstract class LOCOBotModule<T> : ModuleBase<SocketCommandContext>
{
    protected readonly IContext _ctx;
    protected readonly ICommandHandler _commandHandler;
    protected readonly ISettingService _settingService;
    protected readonly ILogger<T> _logger;

    public LOCOBotModule(IContext ctx, ISettingService settingService, ICommandHandler commandHandler, ILogger<T> logger)
    {
        _ctx = ctx;
        _commandHandler = commandHandler;
        _settingService = settingService;
        _logger = logger;
    }

    public static LOCOBotResult FromSuccess(string successMessage = null, IMessage answer = null) => LOCOBotResult.Create(null, successMessage, answer);

    public static LOCOBotResult FromError(CommandError error, string reason, IMessage answer = null) => LOCOBotResult.Create(error, reason, answer);

    public static LOCOBotResult FromErrorObjectNotFound(string objectname, string searchstring, IMessage answer = null) => LOCOBotResult.Create(CommandError.ObjectNotFound, $"{objectname}: {searchstring}", answer);

    public static LOCOBotResult FromErrorUnsuccessful(string error, IMessage answer = null) => LOCOBotResult.Create(CommandError.Unsuccessful, error, answer);

    public static LOCOBotResult FromIgnore() => LOCOBotResult.Create(null, null, null);
}
