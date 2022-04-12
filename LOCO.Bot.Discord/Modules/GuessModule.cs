using Discord;
using Discord.Commands;

using LOCO.Bot.Discord.Helpers;
using LOCO.Bot.Shared.Entities;
using LOCO.Bot.Shared.Modules;
using LOCO.Bot.Shared.Services;

using System.Globalization;

namespace LOCO.Bot.Discord.Modules;

public partial class GuessModule : LOCOBotModule, IMemberGuessModule
{
    public GuessModule(IContext ctx, ISettingService settingService, ICommandHandler commandHandler)
        : base(ctx, settingService, commandHandler)
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

    [Command("StartGuessing")]
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
        var role = Context.Guild.Roles.FirstOrDefault(x => x.Id == settings.GuessMemberRoleId);

        await channel.AddPermissionOverwriteAsync(role, new OverwritePermissions(sendMessages: PermValue.Allow), new RequestOptions { AuditLogReason = $"Guessing started by {Context.User.Username}" });

        settings.GuessingsPossible = true;
        await _settingService.SaveChangesAsync();

        await (channel as ITextChannel).SendMessageAsync("You can start guessing now!");

        return FromSuccess("Guessing is Open now!");
    }

    [Command("StopGuessing")]
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
        var role = Context.Guild.Roles.FirstOrDefault(x => x.Id == settings.GuessMemberRoleId);

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
        var role = Context.Guild.Roles.FirstOrDefault(x => x.Id == settings.GuessMemberRoleId);

        if (Context.Channel.Id != settings.GuessChannelId)
            return FromErrorUnsuccessful("This is the wrong channel for guessings!");

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

            return FromSuccess("Your guess was recored! GLGLGLGLGL");
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

            var closest = _ctx.MemberGuess
                .OrderBy(n => Math.Abs(n.Guess - finalResult))
                .FirstOrDefault();

            var member = Context.Guild.Users.FirstOrDefault(c => c.Id == closest.MemberId);

            await (channel as ITextChannel).SendMessageAsync($"Gratz: {member.Mention} was closest and won!!");

            await _ctx.TruncateAsync("MemberGuess");

            return FromSuccess("Winner was mentioned and Db is clear for next round!");
        }

        return FromError(CommandError.ParseFailed, "Wrong number format.");
    }

    private async Task<LOCOBotResult> CheckAsync(bool checkSettings = true, bool checkIfGuessesAccepted = true)
    {
        var settings = await _settingService.GetSettings(Context?.Guild?.Id ?? 0);

        if (checkSettings && settings.GuessChannelId == 0)
            return FromError(CommandError.Unsuccessful, "No guessing channel configured!");

        if (checkSettings && settings.GuessMemberRoleId == 0)
            return FromError(CommandError.Unsuccessful, "No guessing role configured!");

        if (checkIfGuessesAccepted && !settings.GuessingsPossible)
            return FromError(CommandError.Unsuccessful, "No guessing round active atm!");

        return FromSuccess();
    }
}
