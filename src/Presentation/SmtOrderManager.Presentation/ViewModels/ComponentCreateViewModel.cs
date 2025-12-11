using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SmtOrderManager.Application.Features.Components.Commands.CreateOrUpdateComponent;
using SmtOrderManager.Domain.Entities;

namespace SmtOrderManager.Presentation.ViewModels;

/// <summary>
/// ViewModel for creating new components
/// </summary>
public class ComponentCreateViewModel
{
    private readonly IMediator _mediator;
    private readonly NavigationManager _navigationManager;

    public ComponentCreateViewModel(IMediator mediator, NavigationManager navigationManager)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
    }

    // Form Properties
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; private set; }

    // File Upload State
    public IBrowserFile? SelectedFile { get; private set; }
    public string? FilePreviewUrl { get; private set; }

    // UI State
    public bool IsLoading { get; private set; }
    public bool IsUploading { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? SuccessMessage { get; private set; }

    // Events
    public event Action? StateChanged;

    /// <summary>
    /// Handles file selection
    /// </summary>
    public async Task HandleFileSelectedAsync(IBrowserFile? file)
    {
        SelectedFile = file;
        ErrorMessage = null;

        if (file == null)
        {
            FilePreviewUrl = null;
            NotifyStateChanged();
            return;
        }

        // Validation
        var maxFileSize = 5 * 1024 * 1024; // 5 MB
        if (file.Size > maxFileSize)
        {
            ErrorMessage = "File is too large. Maximum 5 MB allowed.";
            SelectedFile = null;
            FilePreviewUrl = null;
            NotifyStateChanged();
            return;
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.Name).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            ErrorMessage = $"Invalid file format. Allowed: {string.Join(", ", allowedExtensions)}";
            SelectedFile = null;
            FilePreviewUrl = null;
            NotifyStateChanged();
            return;
        }

        // Create preview (as Data URL)
        try
        {
            var buffer = new byte[file.Size];
            await using var stream = file.OpenReadStream(maxFileSize);
            await stream.ReadExactlyAsync(buffer, CancellationToken.None);
            var base64 = Convert.ToBase64String(buffer);
            FilePreviewUrl = $"data:{file.ContentType};base64,{base64}";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading preview: {ex.Message}";
            FilePreviewUrl = null;
        }

        NotifyStateChanged();
    }

    /// <summary>
    /// Creates a new component
    /// </summary>
    public async Task CreateComponentAsync()
    {
        if (!CanCreate())
        {
            ErrorMessage = "Please fill in all required fields.";
            NotifyStateChanged();
            return;
        }

        IsLoading = true;
        ErrorMessage = null;
        SuccessMessage = null;
        NotifyStateChanged();

        try
        {
            // IMPORTANT: Component must be assigned to a board
            // For global components we use a special "Global Board"
            // or set BoardId to a default value
            // Here: We create a component WITHOUT board (BoardId = Guid.Empty or null)
            // This must then be assigned to a board later

            var component = Component.Create(
                Name.Trim(),
                Description.Trim(),
                null // ImageUrl will be set by the command
            );

            Stream? imageStream = null;
            string? imageFileName = null;

            if (SelectedFile != null)
            {
                IsUploading = true;
                NotifyStateChanged();

                imageStream = SelectedFile.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024);
                imageFileName = SelectedFile.Name;
            }

            var command = new CreateOrUpdateComponentCommand(component, imageStream, imageFileName);
            var result = await _mediator.Send(command);

            if (result.Success)
            {
                SuccessMessage = "Component created successfully!";
                NotifyStateChanged();

                // Wait briefly, then navigate to list
                await Task.Delay(1500);
                _navigationManager.NavigateTo("/components");
            }
            else
            {
                ErrorMessage = result.GetError().Message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error creating component: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            IsUploading = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Navigates to component list
    /// </summary>
    public void Cancel()
    {
        _navigationManager.NavigateTo("/components");
    }

    /// <summary>
    /// Checks if the component can be created
    /// </summary>
    public bool CanCreate()
    {
        return !string.IsNullOrWhiteSpace(Name)
               && Name.Trim().Length >= 2
               && !string.IsNullOrWhiteSpace(Description);
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }
}
