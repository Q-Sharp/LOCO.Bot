namespace LOCO.Bot.Shared.Blazor.Auth;

public interface IAuthorizedAntiForgeryClientFactory
{
    Task<HttpClient> CreateClient(string clientName = "authorizedClient");
}
