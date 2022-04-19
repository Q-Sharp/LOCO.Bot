using AspNet.Security.OAuth.Discord;

using LOCO.Bot.Blazor.Server.Auth;
using LOCO.Bot.Data;
using LOCO.Bot.Shared.Blazor.Defaults;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;

using Serilog;

using System.Net;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
var services = builder.Services;
var env = builder.Environment;

configuration.AddEnvironmentVariables("LOCO.Bot_")
             .AddUserSecrets(typeof(Program).Assembly)
             .AddCommandLine(args);

Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

services.AddLogging(l => l.ClearProviders()
                          .AddSerilog(Log.Logger));

var connectionString = configuration.GetConnectionString("Context");

services.AddDbContext<Context>(o => o.UseNpgsql(connectionString));

services.AddSingleton<ITicketStore, LOCOTicketStore>();
services.AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
        .Configure<ITicketStore>((options, store) => options.SessionStore = store);

services.AddHttpClient();
services.AddOptions();

services.AddAuthentication(opt =>
{
    opt.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = DiscordAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
.AddDiscord(DiscordAuthenticationDefaults.AuthenticationScheme, c =>
{
    c.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    c.ClientId = configuration["Discord:AppId"];
    c.ClientSecret = configuration["Discord:AppSecret"];

    c.Events = new OAuthEvents
    {
        OnAccessDenied = context =>
        {
            context.AccessDeniedPath = PathString.FromUriComponent("/");
            context.ReturnUrlParameter = string.Empty;
            return Task.CompletedTask;
        }
    };

    c.SaveTokens = true;
});

services.AddAuthorization(options =>
{
    options.AddPolicy(ApiAuthDefaults.PolicyName, policy =>
    {
        policy.RequireClaim(ClaimTypes.NameIdentifier, "190373435744976896",   // Vielz#9177
                                                       "301764235887902727",   // QoO#1337
                                                       "167671193174802432");  // Shredalexander#7831
    });
});

services.AddControllers();
services.AddRazorPages();

services.AddSession()
        .AddHttpContextAccessor();

services.Configure<ForwardedHeadersOptions>(options =>
{
    options.KnownProxies.Add(IPAddress.Any);
    options.ForwardedHeaders = ForwardedHeaders.All;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseForwardedHeaders()
       .UseCertificateForwarding()
       .UseExceptionHandler("/Error")
       .UseHsts()
       .UseCookiePolicy();
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();

services.AddResponseCompression(options =>
{
    options.Providers.Add<BrotliCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" });
    options.EnableForHttps = true;
});

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        if (context.File.Name == "service-worker-assets.js")
        {
            context.Context.Response.Headers.Add("Cache-Control", "no-cache, no-store");
            context.Context.Response.Headers.Add("Expires", "-1");
        }
    }
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToPage("/_Host");

using var scope = app.Services.CreateScope();
var ctx = scope.ServiceProvider.GetRequiredService<Context>();
await ctx.MigrateAsync();

app.Run();
