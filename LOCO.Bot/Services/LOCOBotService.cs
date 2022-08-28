
using Microsoft.Extensions.Logging;

namespace LOCO.Bot.Services;

public abstract class LOCOBotService<T>
    where T : class
{
    protected ILogger<T> _logger;
    public LOCOBotService(ILogger<T> logger) => _logger = logger;
}
