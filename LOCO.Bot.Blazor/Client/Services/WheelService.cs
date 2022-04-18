using LOCO.Bot.Shared.Blazor.Auth;
using LOCO.Bot.Shared.Blazor.Services;
using LOCO.Bot.Shared.Data.Entities;

using System.Net.Http.Json;

namespace LOCO.Bot.Blazor.Client.Services;

public class WheelService : IWheelService
{
    private readonly ILogger<WheelService> _logger;
    private readonly IAuthorizedAntiForgeryClientFactory _clientFactory;

    public WheelService(ILogger<WheelService> logger, IAuthorizedAntiForgeryClientFactory clientFactory)
    {
        _logger = logger;
        _clientFactory = clientFactory;
    }

    public async Task<ICollection<WheelEntry>> GetWheelEntriesShuffledAsync()
    {
        try
        {
            var http = await _clientFactory.CreateClient();

            var apiWheels = await http.GetFromJsonAsync<WheelEntry[]>("api/wheel");

            var wheels = apiWheels
                .SelectMany(x => Enumerable.Range(1, Math.Abs(x.Qty))
                    .Select(y => new WheelEntry
                    {
                        Color = x.Color,
                        Text = x.Text
                    }));

            return Shuffle(wheels).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ERROR", ex);
        }

        return null;
    }

    private static IEnumerable<T> Shuffle<T>(IEnumerable<T> source)
    {
        var rng = new Random();
        var buffer = source.ToList();

        for (var i = 0; i < buffer.Count; i++)
        {
            var j = rng.Next(i, buffer.Count);
            yield return buffer[j];

            buffer[j] = buffer[i];
        }
    }
}
