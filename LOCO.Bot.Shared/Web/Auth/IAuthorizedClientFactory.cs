namespace LOCO.Bot.Shared.Web.Auth;

public interface IAuthorizedClientFactory
{
    HttpClient CreateClient(string clientName = "authorizedClient");
}
