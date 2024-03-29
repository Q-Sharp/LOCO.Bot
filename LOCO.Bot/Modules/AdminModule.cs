﻿using Discord.Commands;

using LOCO.Bot.Attributes;
using LOCO.Bot.Shared.Discord.Services;

using Microsoft.Extensions.Logging;

namespace LOCO.Bot.Modules;

[Name("Admin")]
public class AdminModule : LOCOBotModule<AdminModule>
{
    private readonly IAdminService _adminService;

    public AdminModule(IContext ctx, ISettingService settingService, ICommandHandler commandHandler, IAdminService adminService, ILogger<AdminModule> logger)
        : base(ctx, settingService, commandHandler, logger) => _adminService = adminService;

    [Command("Restart")]
    [Summary("Restarts the bot")]
    [RequireBotOwner]
    public async Task<RuntimeResult> Restart()
    {
        await ReplyAsync("Restarting.....");
        await _adminService.Restart(Context?.Guild?.Id ?? 0, Context.Channel.Id);
        return FromSuccess();
    }

    [Command("ShowServers")]
    [Summary("Shows all servrs the bot is member of")]
    [RequireBotOwner]
    public async Task<RuntimeResult> ShowServers()
    {
        await ReplyAsync(string.Join(Environment.NewLine, Context.Client.Guilds.Select(x => x.Name)));
        return FromSuccess();
    }
}
