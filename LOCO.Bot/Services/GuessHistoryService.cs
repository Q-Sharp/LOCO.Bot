using LOCO.Bot.Data;
using LOCO.Bot.Shared.Data.Entities;
using LOCO.Bot.Shared.Discord.Services;

namespace LOCO.Bot.Services;

public class GuessHistoryService : IGuessHistoryService
{
    private readonly Context _context;

    public GuessHistoryService(Context context)
    {
        _context = context;
    }

    public async Task CreateHistory(ulong gid)
    {
        _context.Add(new GuessHistory { StartDate = DateTime.Now, GuildId = gid });
        await _context.SaveChangesAsync();
    }

    public async Task UpdateHistoryEnd(ulong gid)
    {
        var gh = _context.GuessHistory.LastOrDefault(x => x.GuildId == gid && x.StartDate != null);
        gh.EndDate = DateTime.Now;
        await _context.SaveChangesAsync();
    }

    public async Task UpdateHistoryResult(ulong gid)
    {
        var gh = _context.GuessHistory.LastOrDefault(x => x.GuildId == gid && x.StartDate != null && x.EndDate != null);
        gh.ResultDate = DateTime.Now;
        gh.Valid = true;
        await _context.SaveChangesAsync();
    }
}