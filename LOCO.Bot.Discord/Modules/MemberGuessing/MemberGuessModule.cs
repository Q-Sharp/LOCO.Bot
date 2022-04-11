using Discord;
using Discord.Commands;

using LOCO.Bot.Data;
using LOCO.Bot.Discord.Helpers;
using LOCO.Bot.Shared.Entities;
using LOCO.Bot.Shared.Modules;
using LOCO.Bot.Shared.Modules.Interfaces;
using LOCO.Bot.Shared.Services.Interfaces;

using System.Globalization;

namespace LOCO.Bot.Discord.Modules.MemberGuessing;

[Name("guess")]
[Group("guess")]
public partial class MemberGuessModule : LOCOBotModule, IMemberGuessModule
{
    public MemberGuessModule(IContext ctx, ISettingService settingService, ICommandHandler commandHandler)
        : base(ctx, settingService, commandHandler)
    {
    }

    [Command("setChannel")]
    [Summary("set guessing channels")]
    [RequireUserPermission(ChannelPermission.ManageMessages)]
    public async Task<RuntimeResult> SetChannel(IChannel guessChannel)
    {
        if (guessChannel is not null)
        {
            _settingService.Settings.GuessChannelId = guessChannel.Id;
            await _settingService.SaveChangesAsync();

            return FromSuccess($"Guess channel set to {guessChannel.Name}");
        }

        return FromError(CommandError.ObjectNotFound, "No channel or channel not found!");
    }

    [Command("setRole")]
    [Summary("set member role for guessings.")]
    [RequireUserPermission(ChannelPermission.ManageMessages)]
    public async Task<RuntimeResult> SetRole(IRole guessRole)
    {
        if (guessRole is not null)
        {
            _settingService.Settings.GuessMemberRoleId = guessRole.Id;
            await _settingService.SaveChangesAsync();

            return FromSuccess($"Guess role set to {guessRole.Name}");
        }

        return FromError(CommandError.ObjectNotFound, "No role or role not found!");
    }

    [Command("start")]
    [Summary("start a new guess session.")]
    [RequireUserPermission(ChannelPermission.ManageMessages)]
    public async Task<RuntimeResult> Start()
    {
        var result = Check(true, false);
        if (result.Error != null)
            return result;

        var channel = (await Context.Guild.GetChannelsAsync()).FirstOrDefault(c => c.Id == _settingService.Settings.GuessChannelId);
        var role = Context.Guild.Roles.FirstOrDefault(x => x.Id == _settingService.Settings.GuessMemberRoleId);

        await channel.AddPermissionOverwriteAsync(role, new OverwritePermissions(sendMessages: PermValue.Allow), new RequestOptions { AuditLogReason = $"Guessing started by {Context.User.Username}" });
        _settingService.Settings.GuessingsPossible = true;
        await _settingService.SaveChangesAsync();

        await (channel as ITextChannel).SendMessageAsync("You can start guessing now!");

        return FromSuccess("Guessing is Open now!");
    }

    [Command("stop")]
    [Summary("stop accepting guesses.")]
    [RequireUserPermission(ChannelPermission.ManageChannels)]
    public async Task<RuntimeResult> Stop()
    {
        var result = Check();
        if (result.Error != null)
            return result;

        var channel = (await Context.Guild.GetChannelsAsync()).FirstOrDefault(c => c.Id == _settingService.Settings.GuessChannelId);
        var role = Context.Guild.Roles.FirstOrDefault(x => x.Id == _settingService.Settings.GuessMemberRoleId);

        await channel.AddPermissionOverwriteAsync(role, new OverwritePermissions(sendMessages: PermValue.Deny), new RequestOptions { AuditLogReason = $"Guessing started by {Context.User.Username}" });
        _settingService.Settings.GuessingsPossible = false;
        await _settingService.SaveChangesAsync();

        await (channel as ITextChannel).SendMessageAsync("Guessings are closed now!");

        return FromSuccess("Guessings are closed now!");
    }

    [Command]
    [Summary("Guess session winnings.")]
    public async Task<RuntimeResult> Guess([Remainder] string guess)
    {
        var result = Check();
        if (result.Error != null)
            return result;

        if (double.TryParse(guess.PrepareForGuessing(), NumberStyles.Currency, new CultureInfo("en-US"), out var dGuess))
        {
            var dbGuess = _ctx.MemberGuess.FirstOrDefault(x => x.MemberId == Context.User.Id);

            if (dbGuess is null)
            {
                dbGuess = _ctx.MemberGuess.Add(new MemberGuess()).Entity;
                dbGuess.MemberName = Context.User.Username;
                dbGuess.MemberId = Context.User.Id;
            }

            dbGuess.Guess = dGuess;

            await _ctx.SaveChangesAsync();

            return FromSuccess("Guess recored!");
        }

        return FromError(CommandError.Unsuccessful, "That wasn't a guess!");
    }

    [Command("end")]
    [Summary("end current guessing session and show winner")]
    public async Task<RuntimeResult> End([Remainder] string endResult)
    {
        var result = Check(true, false);
        if (result.Error != null)
            return result;

        if (double.TryParse(endResult.PrepareForGuessing(), out var finalResult))
        {
            var channel = (await Context.Guild.GetChannelsAsync()).FirstOrDefault(c => c.Id == _settingService.Settings.GuessChannelId);

            var closest = _ctx.MemberGuess
                .OrderBy(n => Math.Abs(n.Guess - finalResult))
                .FirstOrDefault();

            var member = (await Context.Guild.GetUsersAsync()).FirstOrDefault(c => c.Id == closest.MemberId);

            await (channel as ITextChannel).SendMessageAsync($"Gratz: {member.Mention} was closest and won!!");

            await _ctx.TruncateAsync("MemberGuess");

            return FromSuccess("Winner was mentioned and Db is clear for next round!");
        }

        return FromError(CommandError.ParseFailed, "Wrong number format.");
    }

    private LOCOBotResult Check(bool checkSettings = true, bool checkIfGuessesAccepted = true)
    {
        if (checkSettings && _settingService.Settings.GuessChannelId == 0)
            return FromError(CommandError.Unsuccessful, "");

        if (checkSettings && _settingService.Settings.GuessMemberRoleId == 0)
            return FromError(CommandError.Unsuccessful, "");

        if (checkIfGuessesAccepted && !_settingService.Settings.GuessingsPossible)
            return FromError(CommandError.Unsuccessful, "");

        return FromSuccess();
    }
}
