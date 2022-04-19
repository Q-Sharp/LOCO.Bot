namespace LOCO.Bot.Shared.Blazor.Auth;

public interface IAuthorizedClientFactory
{
    HttpClient CreateClient(string clientName = "authorizedClient");
}
