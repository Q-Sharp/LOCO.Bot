using Discord.Commands;

using LOCO.Bot.Discord.Attributes;
using LOCO.Bot.Shared.Services;

namespace LOCO.Bot.Discord.Modules;

[Name("Admin")]
public class AdminModule : LOCOBotModule
{
    private readonly IAdminService _adminService;

    public AdminModule(IContext ctx, ISettingService settingService, ICommandHandler commandHandler, IAdminService adminService) 
        : base(ctx, settingService, commandHandler)
    {
        _adminService = adminService;
    }

    [Command("Restart")]
    [Summary("Restarts the bot")]
    [RequireBotOwner]
    public async Task<RuntimeResult> Restart()
    {
        await ReplyAsync("Restarting.....");
        await _adminService.Restart(Context?.Guild?.Id ?? 0, Context.Channel.Id);
        return FromSuccess();
    }
}
