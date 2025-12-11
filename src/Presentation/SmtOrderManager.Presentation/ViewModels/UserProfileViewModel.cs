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
/// ViewModel for user profile
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
    [Required(ErrorMessage = "Current password is required")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password confirmation is required")]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
    public string ConfirmNewPassword { get; set; } = string.Empty;

    // Events
    public event Action? StateChanged;

    /// <summary>
    /// Loads the user profile of the currently logged in user
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
                ErrorMessage = "Not logged in. Please log in.";
                IsLoading = false;
                NotifyStateChanged();
                return;
            }

            var query = new GetUserByEmailQuery(email);
            var result = await _mediator.Send(query);

            if (result.Success)
            {
                var user = result.GetOk();

                // Convert to UserDto (for security - without PasswordHash)
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
            ErrorMessage = $"Error loading profile: {ex.Message}";
            User = null;
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Changes the password of the current user
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
                SuccessMessage = "Password changed successfully.";

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
            ErrorMessage = $"Error changing password: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Checks if the password can be changed
    /// </summary>
    public bool CanChangePassword()
    {
        return !string.IsNullOrWhiteSpace(CurrentPassword)
               && !string.IsNullOrWhiteSpace(NewPassword)
               && NewPassword.Length >= 6
               && NewPassword == ConfirmNewPassword;
    }

    /// <summary>
    /// Resets the password fields
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
