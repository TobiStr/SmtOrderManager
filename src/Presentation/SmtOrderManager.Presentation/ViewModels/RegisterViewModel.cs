using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.AspNetCore.Components;
using SmtOrderManager.Application.Features.Users.Commands.RegisterUser;

namespace SmtOrderManager.Presentation.ViewModels;

/// <summary>
/// ViewModel für die Registrierung
/// </summary>
public class RegisterViewModel
{
    private readonly IMediator _mediator;
    private readonly NavigationManager _navigationManager;

    public RegisterViewModel(IMediator mediator, NavigationManager navigationManager)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
    }

    // Properties
    [Required(ErrorMessage = "E-Mail ist erforderlich")]
    [EmailAddress(ErrorMessage = "Ungültige E-Mail-Adresse")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name ist erforderlich")]
    [MinLength(2, ErrorMessage = "Name muss mindestens 2 Zeichen lang sein")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Passwort ist erforderlich")]
    [MinLength(6, ErrorMessage = "Passwort muss mindestens 6 Zeichen lang sein")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Passwort-Wiederholung ist erforderlich")]
    [Compare(nameof(Password), ErrorMessage = "Passwörter stimmen nicht überein")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public bool IsLoading { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? SuccessMessage { get; private set; }

    // Events
    public event Action? StateChanged;

    /// <summary>
    /// Führt Registrierung aus
    /// </summary>
    public async Task RegisterAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        SuccessMessage = null;
        NotifyStateChanged();

        try
        {
            var command = new RegisterUserCommand(Email.Trim(), Name.Trim(), Password);
            var result = await _mediator.Send(command);

            if (result.Success)
            {
                SuccessMessage = "Registrierung erfolgreich! Sie werden zum Login weitergeleitet...";
                NotifyStateChanged();

                // Warte 2 Sekunden, dann navigiere zu Login
                await Task.Delay(2000);
                _navigationManager.NavigateTo("/login");
            }
            else
            {
                ErrorMessage = result.GetError().Message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Fehler bei der Registrierung: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Navigiert zum Login
    /// </summary>
    public void NavigateToLogin()
    {
        _navigationManager.NavigateTo("/login");
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }
}
