using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SmtOrderManager.Application.Features.Components.Commands.CreateOrUpdateComponent;
using SmtOrderManager.Application.Features.Components.Commands.DeleteComponent;
using SmtOrderManager.Application.Features.Components.Queries.GetComponentById;
using SmtOrderManager.Domain.Entities;

namespace SmtOrderManager.Presentation.ViewModels;

/// <summary>
/// ViewModel für Component-Details und -Bearbeitung
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

    // Editable Properties (für Bearbeitungsmodus)
    public string EditableName { get; set; } = string.Empty;
    public string EditableDescription { get; set; } = string.Empty;
    public int EditableQuantity { get; set; } = 1;

    // File Upload State
    public IBrowserFile? SelectedFile { get; private set; }
    public string? FilePreviewUrl { get; private set; }
    public bool IsUploading { get; private set; }

    // Events
    public event Action? StateChanged;

    /// <summary>
    /// Lädt Component-Details
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
                EditableQuantity = Component.Quantity;
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
            ErrorMessage = $"Fehler beim Laden der Component: {ex.Message}";
            Component = null;
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Behandelt die Dateiauswahl für Image-Upload
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
            ErrorMessage = $"Ungültiges Dateiformat. Erlaubt: {string.Join(", ", allowedExtensions)}";
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
            ErrorMessage = $"Fehler beim Laden der Vorschau: {ex.Message}";
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
        EditableQuantity = Component.Quantity;
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
    /// Speichert Änderungen an der Component
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
                Quantity = EditableQuantity,
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
                SuccessMessage = "Component erfolgreich gespeichert.";
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
            ErrorMessage = $"Fehler beim Speichern: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            IsUploading = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Löscht die Component
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
            ErrorMessage = $"Fehler beim Löschen: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Navigiert zurück zur Component-Liste
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
    /// Prüft, ob gespeichert werden kann
    /// </summary>
    public bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(EditableName)
               && EditableName.Trim().Length >= 2
               && !string.IsNullOrWhiteSpace(EditableDescription)
               && EditableQuantity > 0;
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }
}
