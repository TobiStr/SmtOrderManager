using MediatR;
using Microsoft.AspNetCore.Components;
using SmtOrderManager.Application.Features.Boards.Commands.CreateOrUpdateBoard;
using SmtOrderManager.Application.Features.Boards.Commands.DeleteBoard;
using SmtOrderManager.Application.Features.Boards.Queries.GetBoardById;
using SmtOrderManager.Application.Features.Components.Commands.CreateOrUpdateComponent;
using SmtOrderManager.Application.Features.Components.Commands.DeleteComponent;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Presentation.Models.DTOs;

namespace SmtOrderManager.Presentation.ViewModels;

/// <summary>
/// ViewModel für Board-Details mit Component-Zuordnung
/// </summary>
public class BoardDetailViewModel
{
    private readonly IMediator _mediator;
    private readonly NavigationManager _navigationManager;

    public BoardDetailViewModel(IMediator mediator, NavigationManager navigationManager)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
    }

    // Properties - Direkt Domain-Entity
    public Board? Board { get; private set; }
    public bool IsLoading { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? SuccessMessage { get; private set; }

    // UI State
    public bool ShowComponentPicker { get; set; }

    // Events
    public event Action? StateChanged;

    /// <summary>
    /// Lädt Board-Details inklusive zugeordneter Components
    /// </summary>
    public async Task LoadAsync(Guid boardId)
    {
        IsLoading = true;
        ErrorMessage = null;
        NotifyStateChanged();

        try
        {
            var query = new GetBoardByIdQuery(boardId);
            var result = await _mediator.Send(query);

            if (result.Success)
            {
                Board = result.GetOk(); // Direkt Domain-Entity
                ErrorMessage = null;
            }
            else
            {
                ErrorMessage = result.GetError().Message;
                Board = null;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Fehler beim Laden des Boards: {ex.Message}";
            Board = null;
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Fügt eine Component zum Board hinzu (aus ComponentPicker)
    /// </summary>
    public async Task AddComponentToBoardAsync(ComponentSelectionResult selection)
    {
        if (Board == null) return;

        IsLoading = true;
        ErrorMessage = null;
        SuccessMessage = null;
        NotifyStateChanged();

        try
        {
            // Component.Create Signatur: (name, description, quantity, boardId, imageUrl)
            var component = Component.Create(
                selection.ComponentName,
                selection.Comment ?? "Component from picker",
                selection.Quantity,
                Board.Id,
                null  // ImageUrl wird aus der globalen Component übernommen
            );

            var command = new CreateOrUpdateComponentCommand(component);
            var result = await _mediator.Send(command);

            if (result.Success)
            {
                SuccessMessage = $"Component '{selection.ComponentName}' erfolgreich hinzugefügt.";
                ShowComponentPicker = false;

                // Reload board to get updated components
                await LoadAsync(Board.Id);
            }
            else
            {
                ErrorMessage = result.GetError().Message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Fehler beim Hinzufügen der Component: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Aktualisiert eine Component auf dem Board
    /// </summary>
    public async Task UpdateComponentAsync(Component component)
    {
        if (Board == null) return;

        IsLoading = true;
        ErrorMessage = null;
        SuccessMessage = null;
        NotifyStateChanged();

        try
        {
            var command = new CreateOrUpdateComponentCommand(component);
            var result = await _mediator.Send(command);

            if (result.Success)
            {
                SuccessMessage = "Component erfolgreich aktualisiert.";
                await LoadAsync(Board.Id);
            }
            else
            {
                ErrorMessage = result.GetError().Message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Fehler beim Aktualisieren der Component: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Entfernt eine Component vom Board
    /// </summary>
    public async Task RemoveComponentFromBoardAsync(Guid componentId)
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
                SuccessMessage = "Component erfolgreich entfernt.";

                if (Board != null)
                {
                    await LoadAsync(Board.Id);
                }
            }
            else
            {
                ErrorMessage = result.GetError().Message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Fehler beim Entfernen der Component: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Löscht das gesamte Board
    /// </summary>
    public async Task DeleteBoardAsync()
    {
        if (Board == null) return;

        IsLoading = true;
        ErrorMessage = null;
        NotifyStateChanged();

        try
        {
            var command = new DeleteBoardCommand(Board.Id);
            var result = await _mediator.Send(command);

            if (result.Success)
            {
                _navigationManager.NavigateTo("/boards");
            }
            else
            {
                ErrorMessage = result.GetError().Message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Fehler beim Löschen des Boards: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Öffnet den Component-Picker Dialog
    /// </summary>
    public void OpenComponentPicker()
    {
        ShowComponentPicker = true;
        NotifyStateChanged();
    }

    /// <summary>
    /// Schließt den Component-Picker Dialog
    /// </summary>
    public void CloseComponentPicker()
    {
        ShowComponentPicker = false;
        NotifyStateChanged();
    }

    // UI-Helper Methods
    public string GetDimensions(Board board) => $"{board.Length} × {board.Width} mm";
    public decimal GetTotalArea(Board board) => board.Length * board.Width;
    public int GetComponentCount(Board board) => board.Components.Count;

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }
}
