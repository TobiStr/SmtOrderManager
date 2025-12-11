using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.AspNetCore.Components;
using SmtOrderManager.Application.Features.Users.Commands.RegisterUser;

namespace SmtOrderManager.Presentation.ViewModels;

/// <summary>
/// ViewModel for registration
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
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name is required")]
    [MinLength(2, ErrorMessage = "Name must be at least 2 characters long")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password confirmation is required")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public bool IsLoading { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? SuccessMessage { get; private set; }

    // Events
    public event Action? StateChanged;

    /// <summary>
    /// Executes registration
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
                SuccessMessage = "Registration successful! Redirecting to login...";
                NotifyStateChanged();

                // Wait 2 seconds, then navigate to login
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
            ErrorMessage = $"Error during registration: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Navigates to login
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
