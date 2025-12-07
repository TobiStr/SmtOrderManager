using MediatR;
using Microsoft.AspNetCore.Components;
using SmtOrderManager.Application.Features.Components.Commands.DeleteComponent;
using SmtOrderManager.Application.Features.Components.Queries.GetAllComponents;
using SmtOrderManager.Domain.Entities;

namespace SmtOrderManager.Presentation.ViewModels;

/// <summary>
/// ViewModel für die Component-Liste (globale Bibliothek)
/// </summary>
public class ComponentListViewModel
{
    private readonly IMediator _mediator;
    private readonly NavigationManager _navigationManager;

    public ComponentListViewModel(IMediator mediator, NavigationManager navigationManager)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
    }

    // Properties - Direkt Domain-Entities
    public List<Component> Components { get; private set; } = new();
    public List<Component> FilteredComponents => ApplyFilter();
    public string? SearchTerm { get; set; }
    public bool IsLoading { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? SuccessMessage { get; private set; }

    // Statistics
    public int TotalCount => Components.Count;
    public int FilteredCount => FilteredComponents.Count;

    // Events
    public event Action? StateChanged;

    /// <summary>
    /// Lädt alle Components aus dem Backend
    /// </summary>
    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        NotifyStateChanged();

        try
        {
            var query = new GetAllComponentsQuery();
            var result = await _mediator.Send(query);

            if (result.Success)
            {
                var components = result.GetOk();
                Components = components.ToList(); // Direkt Domain-Entities
                ErrorMessage = null;
            }
            else
            {
                ErrorMessage = result.GetError().Message;
                Components = new List<Component>();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Fehler beim Laden der Components: {ex.Message}";
            Components = new List<Component>();
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Löscht eine Component
    /// </summary>
    public async Task DeleteComponentAsync(Guid componentId)
    {
        IsLoading = true;
        ErrorMessage = null;
        SuccessMessage = null;
        NotifyStateChanged();

        try
        {
            var command = new DeleteComponentCommand(componentId);
            var result = await _mediator.Send(command);

            if (result.Success)
            {
                SuccessMessage = "Component erfolgreich gelöscht.";
                Components.RemoveAll(c => c.Id == componentId);
            }
            else
            {
                ErrorMessage = result.GetError().Message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Fehler beim Löschen der Component: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Navigiert zur Create-Seite
    /// </summary>
    public void NavigateToCreate()
    {
        _navigationManager.NavigateTo("/components/create");
    }

    /// <summary>
    /// Navigiert zur Detail-Seite einer Component
    /// </summary>
    public void NavigateToDetail(Guid componentId)
    {
        _navigationManager.NavigateTo($"/components/{componentId}");
    }

    /// <summary>
    /// Wendet Suchfilter an
    /// </summary>
    public void ApplySearch()
    {
        NotifyStateChanged();
    }

    /// <summary>
    /// Setzt Suchfilter zurück
    /// </summary>
    public void ClearSearch()
    {
        SearchTerm = null;
        NotifyStateChanged();
    }

    // UI-Helper Methods (statt DTO-Properties)
    public string GetDisplayName(Component component)
        => $"{component.Name} ({component.Description})";

    private List<Component> ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            return Components;
        }

        var searchLower = SearchTerm.ToLowerInvariant();
        return Components.Where(c =>
            c.Name.ToLowerInvariant().Contains(searchLower) ||
            c.Description.ToLowerInvariant().Contains(searchLower)
        ).ToList();
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }
}
