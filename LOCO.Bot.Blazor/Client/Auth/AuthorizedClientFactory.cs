﻿using LOCO.Bot.Shared.Blazor.Auth;

namespace LOCO.Bot.Blazor.Client.Auth;

public class AuthorizedClientFactory : IAuthorizedClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthorizedClientFactory(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public HttpClient CreateClient(string clientName = "authorizedClient") => _httpClientFactory.CreateClient(clientName);
}
