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

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

services.AddDataProtection()
.UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration()
{
    EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
    ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
})
.SetApplicationName("LOCO.Wheel");

var connectionString = configuration.GetConnectionString("Context");

services.AddDbContext<Context>(o => o.UseNpgsql(connectionString));

services.AddSingleton<ITicketStore, LOCOTicketStore>();
services.AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
        .Configure<ITicketStore>((options, store) =>
        {
            options.SessionStore = store;
            options.ExpireTimeSpan = TimeSpan.FromDays(30);
        });

services.AddAntiforgery(options =>
{
    options.HeaderName = AntiforgeryDefaults.Headername;
    options.Cookie.Name = AntiforgeryDefaults.Cookiename;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

services.AddHttpClient();
services.AddOptions();

services.AddAuthentication(opt =>
{
    opt.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = DiscordAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
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

using var scope = app.Services.CreateScope();
var ctx = scope.ServiceProvider.GetRequiredService<Context>();
await ctx.MigrateAsync();

app.Run();
