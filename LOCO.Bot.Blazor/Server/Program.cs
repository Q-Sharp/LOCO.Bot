using System.Net;

using AspNet.Security.OAuth.Discord;

using LOCO.Bot.Blazor.Shared.Defaults;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

using LOCO.Bot.Blazor.Server.Auth;
using LOCO.Bot.Data;

using MudBlazor.Services;

using Serilog;
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
                          .AddSerilog(Log.Logger)
                          .AddSystemdConsole());

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

var connectionString = configuration.GetConnectionString("Context");

services.AddDbContext<Context>(o => o.UseNpgsql(connectionString));

services.AddSingleton<ITicketStore, LOCOTicketStore>();
services.AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
        .Configure<ITicketStore>((options, store) => options.SessionStore = store);

services.AddAntiforgery(options =>
{
    options.HeaderName = AntiforgeryDefaults.Headername;
    options.Cookie.Name = AntiforgeryDefaults.Cookiename;
    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

services.AddHttpClient();
services.AddOptions();

services.AddResponseCaching();

services.AddAuthentication(opt =>
{
    opt.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = DiscordAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, c =>
{
    c.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
    c.ExpireTimeSpan = TimeSpan.FromDays(30);
})
.AddDiscord(DiscordAuthenticationDefaults.AuthenticationScheme, c =>
{
    c.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    c.ClientId = configuration["Discord:AppId"];
    c.ClientSecret = configuration["Discord:AppSecret"];

    c.Events = new OAuthEvents
    {
        //OnCreatingTicket = async context =>
        //{
        //    var guildClaim = await DiscordHelpers.GetGuildClaims(context);
        //    context.Identity.AddClaim(guildClaim);
        //},
        OnAccessDenied = context =>
        {
            context.AccessDeniedPath = PathString.FromUriComponent("/");
            context.ReturnUrlParameter = string.Empty;
            return Task.CompletedTask;
        }
    };

    c.SaveTokens = true;
    //c.Validate();
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

services.AddControllersWithViews(o => o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
services.AddRazorPages();

services.AddMudServices();

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

    app.UseSecurityHeaders(SecurityHeadersDefinitions.GetHeaderPolicyCollection(app.Environment.IsDevelopment(),
                    configuration["Discord:Authority"]));
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();

app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToPage("/_Host");

app.Run();
