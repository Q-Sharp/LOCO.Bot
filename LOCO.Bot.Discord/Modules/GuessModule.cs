using Discord;
using Discord.Commands;

using LOCO.Bot.Discord.Helpers;
using LOCO.Bot.Shared.Entities;
using LOCO.Bot.Shared.Modules;
using LOCO.Bot.Shared.Services;

using Microsoft.Extensions.Logging;

using System.Globalization;

namespace LOCO.Bot.Discord.Modules;

public partial class GuessModule : LOCOBotModule<GuessModule>
{
    public GuessModule(IContext ctx, ISettingService settingService, ICommandHandler commandHandler, ILogger<GuessModule> logger)
        : base(ctx, settingService, commandHandler, logger)
    {

    }

    [Command("setGuessChannel")]
    [Summary("set guessing channels")]
    [RequireUserPermission(ChannelPermission.ManageMessages)]
    public async Task<RuntimeResult> SetChannel(IChannel guessChannel)
    {
        if (guessChannel is not null)
        {
            var settings = await _settingService.GetSettings(Context?.Guild?.Id ?? 0);
            settings.GuessChannelId = guessChannel.Id;
            await _settingService.SaveChangesAsync();

            return FromSuccess($"Guess channel set to {guessChannel.Name}");
        }

        return FromError(CommandError.ObjectNotFound, "No channel or channel not found!");
    }

    [Command("setGuessRole")]
    [Summary("set member role for guessings.")]
    [RequireUserPermission(ChannelPermission.ManageMessages)]
    public async Task<RuntimeResult> SetRole(IRole guessRole)
    {
        if (guessRole is not null)
        {
            var settings = await _settingService.GetSettings(Context?.Guild?.Id ?? 0);
            settings.GuessMemberRoleId = guessRole.Id;
            await _settingService.SaveChangesAsync();

            return FromSuccess($"Guess role set to {guessRole.Name}");
        }

        return FromError(CommandError.ObjectNotFound, "No role or role not found!");
    }

    [Command("removeGuessRole")]
    [Summary("remove role for guessings.")]
    [RequireUserPermission(ChannelPermission.ManageMessages)]
    public async Task<RuntimeResult> RemoveRole()
    {
        var settings = await _settingService.GetSettings(Context?.Guild?.Id ?? 0);
        settings.GuessMemberRoleId = 0;
        await _settingService.SaveChangesAsync();

        return FromSuccess($"Guess role was removed");
    }

    [Command("startGuessing")]
    [Alias("sg")]
    [Summary("start a new guess session.")]
    [RequireUserPermission(ChannelPermission.ManageMessages)]
    public async Task<RuntimeResult> Start()
    {
        var result = await CheckAsync(true, false);
        if (result.Error != null)
            return result;

        var settings = await _settingService.GetSettings(Context?.Guild?.Id ?? 0);

        var channel = Context.Guild.Channels.FirstOrDefault(c => c.Id == settings.GuessChannelId);
        var role = GetRole(settings.GuessMemberRoleId);

        try
        {
            await channel.AddPermissionOverwriteAsync(role, new OverwritePermissions(sendMessages: PermValue.Allow), new RequestOptions { AuditLogReason = $"Guessing started by {Context.User.Username}" });
        }
        catch (Exception ex)
        {
            _logger.LogError("Couldn't overwrite permissions!", ex);
        }

        settings.GuessingsPossible = true;
        await _settingService.SaveChangesAsync();

        await (channel as ITextChannel).SendMessageAsync("You can start guessing now!");

        return FromSuccess("Guessing is Open now!");
    }

    [Command("stopGuessing")]
    [Alias("EndGuess", "eg")]
    [Summary("stop accepting guesses.")]
    [RequireUserPermission(ChannelPermission.ManageChannels)]
    public async Task<RuntimeResult> Stop()
    {
        var result = await CheckAsync();
        if (result.Error != null)
            return result;

        var settings = await _settingService.GetSettings(Context?.Guild?.Id ?? 0);

        var channel = Context.Guild.Channels.FirstOrDefault(c => c.Id == settings.GuessChannelId);
        var role = GetRole(settings.GuessMemberRoleId);

        await channel.AddPermissionOverwriteAsync(role, new OverwritePermissions(sendMessages: PermValue.Deny), new RequestOptions { AuditLogReason = $"Guessing started by {Context.User.Username}" });
        settings.GuessingsPossible = false;
        await _settingService.SaveChangesAsync();

        await (channel as ITextChannel).SendMessageAsync("Guessings are closed now!");

        return FromSuccess("Guessings are closed now!");
    }

    [Command("guess")]
    [Alias("g")]
    [Summary("Guess a win amount! GLGLGLGL")]
    public async Task<RuntimeResult> Guess([Remainder] string guess)
    {
        var result = await CheckAsync();
        if (result.Error != null)
            return result;

        var settings = await _settingService.GetSettings(Context?.Guild?.Id ?? 0);

        var channel = Context.Guild.Channels.FirstOrDefault(c => c.Id == settings.GuessChannelId);
        var role = GetRole(settings.GuessMemberRoleId);

        if (Context.Channel.Id != settings.GuessChannelId)
            return FromErrorUnsuccessful("This is the wrong channel for guessings!");

        if (double.TryParse(guess.PrepareForGuessing(), NumberStyles.Currency, new CultureInfo("en-US"), out var dGuess))
        {
            var dbGuess = _ctx.Guess.FirstOrDefault(x => x.MemberId == Context.User.Id);

            if (dbGuess is null)
            {
                dbGuess = _ctx.Guess.Add(new Guess()).Entity;
                dbGuess.MemberName = Context.User.Username;
                dbGuess.MemberId = Context.User.Id;
            }

            dbGuess.GuessAmount = dGuess;

            await _ctx.SaveChangesAsync();

            return FromSuccess($"Your guess ${dGuess} was recored! GLGLGLGLGL");
        }

        return FromError(CommandError.Unsuccessful, "That wasn't a guess!");
    }

    [Command("PublishResult")]
    [Alias("pr")]
    [Summary("Publish guess session result")]
    public async Task<RuntimeResult> Publish([Remainder] string endResult)
    {
        var result = await CheckAsync(true, false);
        if (result.Error != null)
            return result;

        if (double.TryParse(endResult.PrepareForGuessing(), out var finalResult))
        {
            var settings = await _settingService.GetSettings(Context?.Guild?.Id ?? 0);

            var channel = Context.Guild.Channels.FirstOrDefault(c => c.Id == settings.GuessChannelId);

            var closest = _ctx.Guess
                .OrderBy(n => Math.Abs(n.GuessAmount - finalResult))
                .FirstOrDefault();

            if (closest == null)
            {
                await (channel as ITextChannel).SendMessageAsync($"No guesses == no winners!");
            }
            else
            {
                var member = Context.Guild.Users.FirstOrDefault(c => c.Id == closest.MemberId);

                await (channel as ITextChannel).SendMessageAsync($"Gratz: {member.Mention} was closest and won!!");
                await _ctx.TruncateAsync("MemberGuess");
            }

            return FromSuccess("Winner was mentioned and Db is clear for next round!");
        }

        return FromError(CommandError.ParseFailed, "Wrong number format.");
    }

    private async Task<LOCOBotResult> CheckAsync(bool checkSettings = true, bool checkIfGuessesAccepted = true)
    {
        var settings = await _settingService.GetSettings(Context?.Guild?.Id ?? 0);

        if (checkSettings && settings.GuessChannelId == 0)
            return FromError(CommandError.Unsuccessful, "No guessing channel configured!");

        if (checkIfGuessesAccepted && !settings.GuessingsPossible)
            return FromError(CommandError.Unsuccessful, "No guessing round active atm!");

        return FromSuccess();
    }

    private IRole GetRole(ulong roleid)
        => roleid == 0 ? Context.Guild.EveryoneRole
            : Context.Guild.Roles.FirstOrDefault(x => x.Id == roleid);
}
