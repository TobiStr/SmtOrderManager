using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SmtOrderManager.Presentation.Models.DTOs;
using SmtOrderManager.Presentation.Services;

namespace SmtOrderManager.Presentation.ViewModels;

/// <summary>
/// ViewModel für den Login
/// </summary>
public class LoginViewModel
{
    private readonly IJSRuntime _jsRuntime;
    private readonly NavigationManager _navigationManager;
    private readonly CustomAuthenticationStateProvider _authStateProvider;

    public LoginViewModel(
        IJSRuntime jsRuntime,
        NavigationManager navigationManager,
        CustomAuthenticationStateProvider authStateProvider)
    {
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
        _authStateProvider = authStateProvider ?? throw new ArgumentNullException(nameof(authStateProvider));
    }

    // Properties
    [Required(ErrorMessage = "E-Mail ist erforderlich")]
    [EmailAddress(ErrorMessage = "Ungültige E-Mail-Adresse")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Passwort ist erforderlich")]
    [MinLength(6, ErrorMessage = "Passwort muss mindestens 6 Zeichen lang sein")]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }

    public bool IsLoading { get; private set; }
    public string? ErrorMessage { get; private set; }

    // Events
    public event Action? StateChanged;

    /// <summary>
    /// Führt Login aus
    /// </summary>
    public async Task LoginAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        NotifyStateChanged();

        try
        {
            var result = await _jsRuntime.InvokeAsync<LoginResponse>("smtAuth.login", new LoginRequest
            {
                Email = Email.Trim(),
                Password = Password,
                RememberMe = RememberMe
            });

            if (result.Success && result.User is not null)
            {
                await _authStateProvider.MarkUserAsAuthenticated(result.User, RememberMe);

                _navigationManager.NavigateTo("/", forceLoad: true);
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Login fehlgeschlagen. Bitte überprüfen Sie Ihre Zugangsdaten.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Fehler beim Login: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Navigiert zur Registrierung
    /// </summary>
    public void NavigateToRegister()
    {
        _navigationManager.NavigateTo("/register");
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }

    private sealed class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }

    private sealed class LoginResponse
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public UserDto? User { get; set; }
    }
}
