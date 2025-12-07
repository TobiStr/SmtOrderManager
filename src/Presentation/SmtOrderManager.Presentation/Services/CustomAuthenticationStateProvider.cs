using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;
using SmtOrderManager.Presentation.Models.DTOs;

namespace SmtOrderManager.Presentation.Services;

/// <summary>
/// Einfacher, selbst gebauter AuthenticationStateProvider
/// OHNE komplettes ASP.NET Core Identity Framework
/// </summary>
public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    public const string Scheme = "CustomAuth";

    private readonly ProtectedSessionStorage _sessionStorage;
    private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

    public CustomAuthenticationStateProvider(
        ProtectedSessionStorage sessionStorage)
    {
        _sessionStorage = sessionStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var userSessionResult = await _sessionStorage.GetAsync<UserDto>("UserSession");
            var userSession = userSessionResult.Success ? userSessionResult.Value : null;

            if (userSession == null)
                return await Task.FromResult(new AuthenticationState(_anonymous));

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userSession.Id.ToString()),
                new Claim(ClaimTypes.Name, userSession.Name),
                new Claim(ClaimTypes.Email, userSession.Email)
            }, Scheme));

            return await Task.FromResult(new AuthenticationState(claimsPrincipal));
        }
        catch
        {
            return await Task.FromResult(new AuthenticationState(_anonymous));
        }
    }

    public async Task MarkUserAsAuthenticated(UserDto user, bool isPersistent = false)
    {
        await _sessionStorage.SetAsync("UserSession", user);

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email)
        }, Scheme));

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
    }

    public async Task MarkUserAsLoggedOut()
    {
        await _sessionStorage.DeleteAsync("UserSession");
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }
}
