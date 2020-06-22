using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace server
{
  public class Startup
  {
    public Startup(IConfiguration config)
    {
      Configuration = config;
    }

    public IConfiguration Configuration { get; set; }
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
          .AddCookie(o => o.LoginPath = new PathString("/login"))
          .AddOpenIdConnect("Auth0", o =>
          {
            o.SaveTokens = true;
            o.GetClaimsFromUserInfoEndpoint = true;
            o.ClientId = Configuration["Auth0:ClientId"];
            o.ClientSecret = Configuration["Auth0:ClientSecret"];
            o.CallbackPath = new PathString("/callback");
            o.Authority = $"https://{Configuration["Auth0:Domain"]}";
            o.ResponseType = OpenIdConnectResponseType.CodeIdToken;

            o.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
            o.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");

            // Configure the scopes
            foreach (var scope in Configuration["Auth0:Scopes"].Split(" "))
            {
              o.Scope.Add(scope);
            }
          })
          .AddOAuth("github", "Github", o =>
          {
            o.SaveTokens = true;
            o.ClientId = Configuration["Github:ClientId"];
            o.ClientSecret = Configuration["Github:ClientSecret"];
            o.CallbackPath = new PathString("/authorize-github");
            o.AuthorizationEndpoint = Configuration["Github:AuthorizationEndpoint"];
            o.TokenEndpoint = Configuration["Github:TokenEndpoint"];

             foreach (var scope in Configuration["Github:Scopes"].Split(" "))
            {
              o.Scope.Add(scope);
            }
            //possible these events could be used to cache tokens
            //somewhere else.
            o.Events.OnTicketReceived = ctx => Task.CompletedTask;
            o.Events.OnCreatingTicket = ctx => Task.CompletedTask;
          })
          .AddOAuth("twitch", "Twitch.tv", o =>
          {
            o.SaveTokens = true;
            o.ClientId = Configuration["Twitch:ClientId"];
            o.ClientSecret = Configuration["Twitch:ClientSecret"];
            o.CallbackPath = new PathString("/authorize-twitch");
            o.AuthorizationEndpoint = Configuration["Twitch:AuthorizationEndpoint"];
            o.TokenEndpoint = Configuration["Twitch:TokenEndpoint"];

            // Configure the scopes
            foreach (var scope in Configuration["Twitch:Scopes"].Split(" "))
            {
              o.Scope.Add(scope);
            }
            //possible these events could be used to cache tokens
            //somewhere else.
            o.Events.OnTicketReceived = ctx => Task.CompletedTask;
            o.Events.OnCreatingTicket = ctx => Task.CompletedTask;
          });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {

      app.UseDeveloperExceptionPage();
      app.UseAuthentication();

      app.Map("/login", _ =>
      {
        _.Run(async context =>
        {
          var authType = context.Request.Query["authscheme"];
          if (!string.IsNullOrWhiteSpace(authType))
          {
            await context.ChallengeAsync(authType,
              new AuthenticationProperties() { RedirectUri = "/" });
            return;
          }

          var response = context.Response;
          var schemeProvider = context.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();

          response.ContentType = "text/html";
          await response.WriteAsync("<html><body>");
          await response.WriteAsync("Choose an auth scheme: <br>");
          foreach (var provider in await schemeProvider.GetAllSchemesAsync())
          {
            if (!string.IsNullOrWhiteSpace(provider.DisplayName))
              await response.WriteAsync($"<a href='?authscheme={provider.Name}'>{provider.DisplayName}</a><br>");
          }
          await response.WriteAsync("</body></html>");
        });
      });

      app.Map("/logout", _ =>
      {
        _.Run(async context =>
        {
          var response = context.Response;
          response.ContentType = "text/html";
          await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
          await response.WriteAsync("<html><body>");
          await response.WriteAsync($"You have been logged out. Goodbye {context.User.Identity.Name}<br>");
          await response.WriteAsync("<a href='/'>Home</a>");
          await response.WriteAsync("</body></html>");
        });
      });

      app.Run(async context =>
      {
        var user = context.User;
        if (user == null || !user.Identities.Any(identity => identity.IsAuthenticated))
        {
          await context.ChallengeAsync();
          return;
        }

        var response = context.Response;
        var schemeProvider = context.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
        response.ContentType = "text/html";
        await response.WriteAsync("<html><body>");
        await response.WriteAsync($"<h1>Hello {context.User.Identity.Name ?? "Anonymous"}</h1>");

        if(context.User.Identity.IsAuthenticated)
        {
          await response.WriteAsync($"<p><a href='/login'>Authrorize</a></p>");
        }

        await response.WriteAsync("<h2>Claims:</h2>");
        foreach (var claim in context.User.Claims)
        {
          await response.WriteAsync($"{claim.Type}:{claim.Value}<br>");
        }

        await response.WriteAsync("<h2>Tokens:</h2>");
        foreach (var provider in await schemeProvider.GetAllSchemesAsync())
        {
          if (string.IsNullOrWhiteSpace(provider.DisplayName))
            continue;
          await response.WriteAsync($"<h3>Provider: {provider.DisplayName}</h3>");
          await response.WriteAsync($"Access Token: {await context.GetTokenAsync(provider.Name, "access_token")}<br>");
          await response.WriteAsync($"Refresh Token: {await context.GetTokenAsync(provider.Name, "refresh_token")}<br>");
          await response.WriteAsync($"Token Type: {await context.GetTokenAsync(provider.Name, "token_type")}<br>");
          await response.WriteAsync($"expires_at: {await context.GetTokenAsync(provider.Name, "expires_at")}<br><br>");
        }
      });
    }
  }
}
