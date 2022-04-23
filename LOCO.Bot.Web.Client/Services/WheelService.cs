using LOCO.Bot.Shared.Data.Entities;
using LOCO.Bot.Shared.Web.Auth;
using LOCO.Bot.Shared.Web.Extensions;
using LOCO.Bot.Shared.Web.Services;

using System.Net.Http.Json;

namespace LOCO.Bot.Web.Client.Services;

public class WheelService : IWheelService
{
    private readonly ILogger<WheelService> _logger;
    private readonly IAuthorizedClientFactory _clientFactory;

    public WheelService(ILogger<WheelService> logger, IAuthorizedClientFactory clientFactory)
    {
        _logger = logger;
        _clientFactory = clientFactory;
    }

    public async Task<IEnumerable<WheelEntry>> GetWheelEntriesShuffledAsync()
    {
        try
        {
            var http = _clientFactory.CreateClient();
            var apiWheels = await http.GetFromJsonAsync<WheelEntry[]>("api/wheel");

            return apiWheels
                .SelectMany(x => Enumerable.Range(1, Math.Abs(x.Qty))
                .Select(y => new WheelEntry
                {
                    Color = x.Color,
                    Text = x.Text
                }))
                .Shuffle();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ERROR", ex);
        }

        return null;
    }

    public async Task<IEnumerable<WheelEntry>> GetNextWheelEntries(int qty)
    {
        var wheels = (await GetWheelEntriesShuffledAsync()).ToList();

        while(wheels.Count < qty)
        {
            var rng = Random.Shared.Next(0, wheels.Count - 1);
            var we = new WheelEntry();
            we.Update(wheels[rng]);
            wheels.Add(we);
        }

        return wheels.Take(qty);
    }
}
