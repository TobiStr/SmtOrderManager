using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SmtOrderManager.Application.Features.Components.Commands.CreateOrUpdateComponent;
using SmtOrderManager.Domain.Entities;

namespace SmtOrderManager.Presentation.ViewModels;

/// <summary>
/// ViewModel für die Erstellung neuer Components
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
    /// Behandelt die Dateiauswahl
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

        // Preview erstellen (als Data URL)
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
    /// Erstellt eine neue Component
    /// </summary>
    public async Task CreateComponentAsync()
    {
        if (!CanCreate())
        {
            ErrorMessage = "Bitte füllen Sie alle Pflichtfelder aus.";
            NotifyStateChanged();
            return;
        }

        IsLoading = true;
        ErrorMessage = null;
        SuccessMessage = null;
        NotifyStateChanged();

        try
        {
            // WICHTIG: Component muss einem Board zugeordnet sein
            // Für globale Components verwenden wir ein spezielles "Global Board"
            // oder setzen BoardId auf einen Standardwert
            // Hier: Wir erstellen eine Component OHNE Board (BoardId = Guid.Empty oder null)
            // Dies muss dann später einem Board zugeordnet werden

            var component = Component.Create(
                Name.Trim(),
                Description.Trim(),
                null // ImageUrl wird vom Command gesetzt
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
                SuccessMessage = "Component erfolgreich erstellt!";
                NotifyStateChanged();

                // Kurz warten, dann zur Liste navigieren
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
            ErrorMessage = $"Fehler beim Erstellen der Component: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            IsUploading = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Navigiert zur Component-Liste
    /// </summary>
    public void Cancel()
    {
        _navigationManager.NavigateTo("/components");
    }

    /// <summary>
    /// Prüft, ob die Component erstellt werden kann
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
