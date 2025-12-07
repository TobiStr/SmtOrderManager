using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.AspNetCore.Components;
using SmtOrderManager.Application.Features.Users.Commands.LoginUser;
using SmtOrderManager.Presentation.Models.DTOs;
using SmtOrderManager.Presentation.Services;

namespace SmtOrderManager.Presentation.ViewModels;

/// <summary>
/// ViewModel für den Login
/// </summary>
public class LoginViewModel
{
    private readonly IMediator _mediator;
    private readonly NavigationManager _navigationManager;
    private readonly CustomAuthenticationStateProvider _authStateProvider;

    public LoginViewModel(
        IMediator mediator,
        NavigationManager navigationManager,
        CustomAuthenticationStateProvider authStateProvider)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
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
            var command = new LoginUserCommand(Email.Trim(), Password);
            var result = await _mediator.Send(command);

            if (result.Success)
            {
                var user = result.GetOk();

                // User in UserDto konvertieren (für Sicherheit - ohne PasswordHash)
                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    LastLoginAt = user.LastLoginAt,
                    CreatedAt = user.CreatedAt
                };

                // User als authentifiziert markieren
                await _authStateProvider.MarkUserAsAuthenticated(userDto);

                _navigationManager.NavigateTo("/");
            }
            else
            {
                ErrorMessage = "Login fehlgeschlagen. Bitte überprüfen Sie Ihre Zugangsdaten.";
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
}
