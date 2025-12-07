using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.AspNetCore.Components;
using SmtOrderManager.Application.Features.Users.Commands.UpdateUserPassword;
using SmtOrderManager.Application.Features.Users.Queries.GetUserByEmail;
using SmtOrderManager.Application.Interfaces;
using SmtOrderManager.Presentation.Models;
using SmtOrderManager.Presentation.Models.DTOs;

namespace SmtOrderManager.Presentation.ViewModels;

/// <summary>
/// ViewModel für das User-Profil
/// </summary>
public class UserProfileViewModel
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly NavigationManager _navigationManager;

    public UserProfileViewModel(
        IMediator mediator,
        ICurrentUserService currentUserService,
        NavigationManager navigationManager)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
    }

    // Properties
    public UserDto? User { get; private set; }
    public bool IsLoading { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? SuccessMessage { get; private set; }

    // Password Change Properties
    [Required(ErrorMessage = "Aktuelles Passwort ist erforderlich")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Neues Passwort ist erforderlich")]
    [MinLength(6, ErrorMessage = "Passwort muss mindestens 6 Zeichen lang sein")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Passwort-Wiederholung ist erforderlich")]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwörter stimmen nicht überein")]
    public string ConfirmNewPassword { get; set; } = string.Empty;

    // Events
    public event Action? StateChanged;

    /// <summary>
    /// Lädt das User-Profil des aktuell angemeldeten Users
    /// </summary>
    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        NotifyStateChanged();

        try
        {
            var email = _currentUserService.GetEmail();
            if (string.IsNullOrWhiteSpace(email))
            {
                ErrorMessage = "Nicht angemeldet. Bitte melden Sie sich an.";
                IsLoading = false;
                NotifyStateChanged();
                return;
            }

            var query = new GetUserByEmailQuery(email);
            var result = await _mediator.Send(query);

            if (result.Success)
            {
                var user = result.GetOk();

                // Convert to UserDto (für Sicherheit - ohne PasswordHash)
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    LastLoginAt = user.LastLoginAt,
                    CreatedAt = user.CreatedAt
                };

                ErrorMessage = null;
            }
            else
            {
                ErrorMessage = result.GetError().Message;
                User = null;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Fehler beim Laden des Profils: {ex.Message}";
            User = null;
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Ändert das Passwort des aktuellen Users
    /// </summary>
    public async Task ChangePasswordAsync()
    {
        if (User == null) return;

        IsLoading = true;
        ErrorMessage = null;
        SuccessMessage = null;
        NotifyStateChanged();

        try
        {
            var command = new UpdateUserPasswordCommand(User.Id, CurrentPassword, NewPassword);
            var result = await _mediator.Send(command);

            if (result.Success)
            {
                SuccessMessage = "Passwort erfolgreich geändert.";

                // Reset password fields
                CurrentPassword = string.Empty;
                NewPassword = string.Empty;
                ConfirmNewPassword = string.Empty;
            }
            else
            {
                ErrorMessage = result.GetError().Message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Fehler beim Ändern des Passworts: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Prüft, ob das Passwort geändert werden kann
    /// </summary>
    public bool CanChangePassword()
    {
        return !string.IsNullOrWhiteSpace(CurrentPassword)
               && !string.IsNullOrWhiteSpace(NewPassword)
               && NewPassword.Length >= 6
               && NewPassword == ConfirmNewPassword;
    }

    /// <summary>
    /// Setzt die Passwort-Felder zurück
    /// </summary>
    public void ResetPasswordFields()
    {
        CurrentPassword = string.Empty;
        NewPassword = string.Empty;
        ConfirmNewPassword = string.Empty;
        ErrorMessage = null;
        SuccessMessage = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }
}
