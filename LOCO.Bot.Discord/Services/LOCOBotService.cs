namespace LOCO.Bot.Discord.Services;

public abstract class LOCOBotService<T>
    where T : class
{
    protected ILogger<T> _logger;
    public LOCOBotService(ILogger<T> logger) => _logger = logger;
}
