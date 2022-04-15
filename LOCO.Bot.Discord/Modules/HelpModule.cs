using Discord;
using Discord.Commands;

using LOCO.Bot.Data;
using LOCO.Bot.Discord.Attributes;
using LOCO.Bot.Shared.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LOCO.Bot.Discord.Modules;

[Name("Help")]
public class HelpModule : LOCOBotModule<HelpModule>
{
    private readonly CommandService _service;
    private readonly IConfiguration _configuration;

    public HelpModule(CommandService service, Context ctx, ISettingService settingService, ICommandHandler commandHandler, IConfiguration configuration, ILogger<HelpModule> logger)
        : base(ctx, settingService, commandHandler, logger)
    {
        _service = service;
        _configuration = configuration;
    }

    [Command("help")]
    public async Task<RuntimeResult> HelpAsync()
    {
        var prefix = _configuration["Discord:Prefix"];

        var builder = new EmbedBuilder()
        {
            Color = new Color(114, 137, 218),
            Description = "These are the commands you can use:",
            Footer = new EmbedFooterBuilder()
            {
                Text = "To get more information for each command add the command name behind the help command!"
            }
        };

        foreach (var module in _service.Modules)
        {
            string description = null;
            foreach (var cmd in module.Commands.Where(x => x.Preconditions.All(attribute => attribute.GetType() != typeof(RequireOwnerAttribute) 
                && attribute.GetType() != typeof(RequireBotOwnerAttribute))).Distinct().ToArray())
            {
                var result = await cmd.CheckPreconditionsAsync(Context);
                if (!result.IsSuccess)
                    continue;

                var args = string.Join(" ", cmd.Parameters?.Select(x => $"[{x.Name}]").ToArray() ?? Array.Empty<string>());

                if (string.Equals(cmd.Name, module.Group, StringComparison.InvariantCultureIgnoreCase))
                    description += $"{prefix}{module.Group} {args}{Environment.NewLine}";
                else if (string.IsNullOrWhiteSpace(module.Group))
                    description += $"{prefix}{cmd.Name} {args}{Environment.NewLine}";
                else
                    description += $"{prefix}{module.Group} {cmd.Name} {args}{Environment.NewLine}";
            }

            if (!string.IsNullOrWhiteSpace(description))
                builder.AddField(x =>
                {
                    x.Name = module.Name;
                    x.Value = description;
                    x.IsInline = false;
                });
        }

        await ReplyAsync("", false, builder.Build());
        return FromSuccess();
    }

    [Command("help")]
    public async Task<RuntimeResult> HelpAsync([Remainder] string command)
    {
        var result = _service.Search(Context, command);

        if (!result.IsSuccess)
            return FromErrorObjectNotFound("command", command);

        var builder = new EmbedBuilder()
        {
            Color = new Color(114, 137, 218),
            Description = $"Here are some commands like **{command}**"
        };

        foreach (var match in result.Commands)
        {
            var cmd = match.Command;

            builder.AddField(x =>
            {
                x.Name = string.Join(", ", cmd.Aliases);
                x.Value = $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n" +
                          $"Summary: {cmd.Summary}";
                x.IsInline = false;
            });
        }

        await ReplyAsync("", false, builder.Build());
        return FromSuccess();
    }
}
