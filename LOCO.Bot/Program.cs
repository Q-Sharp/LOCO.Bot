using LOCO.Bot.Discord;

using Microsoft.Extensions.Hosting;

using Serilog.Debugging;

SelfLog.Enable(l => Console.WriteLine(l));

try
{
    using var hb = DiscordSocketHost.CreateDiscordSocketHost(args)?.Build();
    await hb.RunAsync();
}
catch (Exception e)
{
    SelfLog.WriteLine("{e}", e);
}
