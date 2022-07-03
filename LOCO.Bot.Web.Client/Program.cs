var builder = WebAssemblyHostBuilder.CreateDefault(args);

Log.Logger = new LoggerConfiguration()
                .WriteTo.BrowserConsole()
                .CreateLogger();

builder.Logging.ClearProviders();

if (builder.HostEnvironment.IsDevelopment())
    builder.Logging.AddSerilog(Log.Logger);

builder.Services.AddOptions();
builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy(ApiAuthDefaults.PolicyName, policy =>
    {
        policy.RequireClaim(ClaimTypes.NameIdentifier, "190373435744976896",  // Vielz#9177
                                                       "301764235887902727",  // QoO#1337
                                                       "167671193174802432"); // Shredalexander#7831
    });
});

builder.Services.TryAddSingleton<AuthenticationStateProvider, LOCOAuthenticationStateProvider>();
builder.Services.TryAddSingleton(sp => (LOCOAuthenticationStateProvider)sp.GetRequiredService<AuthenticationStateProvider>());
builder.Services.AddTransient<AuthorizedHandler>();

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient("default", client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddHttpClient("authorizedClient", client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
}).AddHttpMessageHandler<AuthorizedHandler>();

builder.Services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("default"));

builder.Services.AddTransient<IAuthorizedClientFactory, AuthorizedClientFactory>();

builder.Services.AddBlazoredSessionStorage();
builder.Services.AddMudServices();
builder.Services.AddTransient<IWheelService, WheelService>();

await builder.Build().RunAsync();
