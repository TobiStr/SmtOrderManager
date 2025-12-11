using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SmtOrderManager.Presentation.Models.DTOs;
using SmtOrderManager.Presentation.Services;

namespace SmtOrderManager.Presentation.ViewModels;

/// <summary>
/// ViewModel for login
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
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }

    public bool IsLoading { get; private set; }
    public string? ErrorMessage { get; private set; }

    // Events
    public event Action? StateChanged;

    /// <summary>
    /// Executes login
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
                ErrorMessage = result.ErrorMessage ?? "Login failed. Please check your credentials.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error during login: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Navigates to registration
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
