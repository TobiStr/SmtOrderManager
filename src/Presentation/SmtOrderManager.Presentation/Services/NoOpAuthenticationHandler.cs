using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace SmtOrderManager.Presentation.Services;

/// <summary>
/// Minimal authentication handler to satisfy ASP.NET Core auth pipeline when using a custom AuthenticationStateProvider.
/// </summary>
internal sealed class NoOpAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public NoOpAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Context.User?.Identity?.IsAuthenticated == true)
        {
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(Context.User, Scheme.Name)));
        }

        return Task.FromResult(AuthenticateResult.NoResult());
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    }
}
