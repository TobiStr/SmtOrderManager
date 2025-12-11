using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SmtOrderManager.Application.Features.Components.Commands.CreateOrUpdateComponent;
using SmtOrderManager.Application.Features.Components.Commands.DeleteComponent;
using SmtOrderManager.Application.Features.Components.Queries.GetComponentById;
using SmtOrderManager.Domain.Entities;

namespace SmtOrderManager.Presentation.ViewModels;

/// <summary>
/// ViewModel for component details and editing
/// </summary>
public class ComponentDetailViewModel
{
    private readonly IMediator _mediator;
    private readonly NavigationManager _navigationManager;

    public ComponentDetailViewModel(IMediator mediator, NavigationManager navigationManager)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
    }

    // Properties
    public Component? Component { get; private set; }
    public bool IsLoading { get; private set; }
    public bool IsEditMode { get; set; }
    public string? ErrorMessage { get; private set; }
    public string? SuccessMessage { get; private set; }

    // Editable Properties (for edit mode)
    public string EditableName { get; set; } = string.Empty;
    public string EditableDescription { get; set; } = string.Empty;
    // File Upload State
    public IBrowserFile? SelectedFile { get; private set; }
    public string? FilePreviewUrl { get; private set; }
    public bool IsUploading { get; private set; }

    // Events
    public event Action? StateChanged;

    /// <summary>
    /// Loads component details
    /// </summary>
    public async Task LoadAsync(Guid componentId)
    {
        IsLoading = true;
        ErrorMessage = null;
        NotifyStateChanged();

        try
        {
            var query = new GetComponentByIdQuery(componentId);
            var result = await _mediator.Send(query);

            if (result.Success)
            {
                Component = result.GetOk();

                // Editable Properties initialisieren
                EditableName = Component.Name;
                EditableDescription = Component.Description;
                IsEditMode = false;
            }
            else
            {
                ErrorMessage = result.GetError().Message;
                Component = null;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading component: {ex.Message}";
            Component = null;
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Handles file selection for image upload
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

        // Validierung
        var maxFileSize = 5 * 1024 * 1024; // 5 MB
        if (file.Size > maxFileSize)
        {
            ErrorMessage = "Die Datei ist zu groß. Maximal 5 MB erlaubt.";
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

        // Preview erstellen
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
    /// Aktiviert den Bearbeitungsmodus
    /// </summary>
    public void EnableEditMode()
    {
        if (Component == null) return;

        IsEditMode = true;
        EditableName = Component.Name;
        EditableDescription = Component.Description;
        NotifyStateChanged();
    }

    /// <summary>
    /// Bricht die Bearbeitung ab
    /// </summary>
    public void CancelEdit()
    {
        IsEditMode = false;
        SelectedFile = null;
        FilePreviewUrl = null;
        ErrorMessage = null;
        NotifyStateChanged();
    }

    /// <summary>
    /// Saves changes to the component
    /// </summary>
    public async Task SaveAsync()
    {
        if (Component == null) return;

        IsLoading = true;
        ErrorMessage = null;
        SuccessMessage = null;
        NotifyStateChanged();

        try
        {
            // Component mit neuen Werten erstellen (record with)
            var updatedComponent = Component with
            {
                Name = EditableName.Trim(),
                Description = EditableDescription.Trim(),
                UpdatedAt = DateTime.UtcNow
            };

            Stream? imageStream = null;
            string? imageFileName = null;

            if (SelectedFile != null)
            {
                IsUploading = true;
                NotifyStateChanged();

                imageStream = SelectedFile.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024);
                imageFileName = SelectedFile.Name;
            }

            var command = new CreateOrUpdateComponentCommand(updatedComponent, imageStream, imageFileName);
            var result = await _mediator.Send(command);

            if (result.Success)
            {
                Component = result.GetOk();
                SuccessMessage = "Component saved successfully.";
                IsEditMode = false;
                SelectedFile = null;
                FilePreviewUrl = null;
            }
            else
            {
                ErrorMessage = result.GetError().Message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error saving: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            IsUploading = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Deletes the component
    /// </summary>
    public async Task DeleteAsync()
    {
        if (Component == null) return;

        IsLoading = true;
        ErrorMessage = null;
        NotifyStateChanged();

        try
        {
            var command = new DeleteComponentCommand(Component.Id);
            var result = await _mediator.Send(command);

            if (result.Success)
            {
                _navigationManager.NavigateTo("/components");
            }
            else
            {
                ErrorMessage = result.GetError().Message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error deleting: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Navigates back to component list
    /// </summary>
    public void NavigateBack()
    {
        _navigationManager.NavigateTo("/components");
    }

    /// <summary>
    /// Gibt die Image-URL zurück (mit Fallback)
    /// </summary>
    public string GetImageUrl()
    {
        if (!string.IsNullOrWhiteSpace(FilePreviewUrl))
            return FilePreviewUrl;

        if (!string.IsNullOrWhiteSpace(Component?.ImageUrl))
            return Component.ImageUrl;

        return "/images/placeholder-component.png";
    }

    /// <summary>
    /// Checks if save is possible
    /// </summary>
    public bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(EditableName)
               && EditableName.Trim().Length >= 2
               && !string.IsNullOrWhiteSpace(EditableDescription);
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }
}
