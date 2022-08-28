using AspNet.Security.OAuth.Discord;

using LOCO.Bot.Data;
using LOCO.Bot.Shared.Blazor.Defaults;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.HttpOverrides;
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

services.AddHttpClient();
services.AddOptions();

services.AddLocalization();

services.AddAuthentication(opt =>
{
    opt.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = DiscordAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.Cookie.MaxAge = TimeSpan.FromDays(30);
    options.Cookie.Name = ApiAuthDefaults.CookieName;
})
.AddDiscord(DiscordAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.ClientId = configuration["Discord:AppId"];
    options.ClientSecret = configuration["Discord:AppSecret"];

    options.Events = new OAuthEvents
    {
        OnAccessDenied = context =>
        {
            context.AccessDeniedPath = PathString.FromUriComponent("/");
            context.ReturnUrlParameter = string.Empty;
            return Task.CompletedTask;
        }
    };

    options.SaveTokens = true;
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

services.AddResponseCompression(options =>
{
    options.Providers.Add<BrotliCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" });
    options.EnableForHttps = true;
});

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
app.UseRequestLocalization("en-US");

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
