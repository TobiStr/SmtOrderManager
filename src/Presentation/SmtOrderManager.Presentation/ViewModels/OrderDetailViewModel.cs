using MediatR;
using Microsoft.AspNetCore.Components;
using SmtOrderManager.Application.Features.Orders.Commands.AddBoardToOrder;
using SmtOrderManager.Application.Features.Orders.Commands.CreateOrUpdateOrder;
using SmtOrderManager.Application.Features.Orders.Commands.DeleteOrder;
using SmtOrderManager.Application.Features.Orders.Queries.GetOrderById;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Enums;
using SmtOrderManager.Presentation.Components.Shared;

namespace SmtOrderManager.Presentation.ViewModels;

/// <summary>
/// ViewModel für Order-Details
/// </summary>
public class OrderDetailViewModel
{
    private readonly IMediator _mediator;
    private readonly NavigationManager _navigationManager;

    public OrderDetailViewModel(IMediator mediator, NavigationManager navigationManager)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
    }

    // Properties - Direkt Domain-Entity
    public Order? Order { get; private set; }
    public bool IsLoading { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? SuccessMessage { get; private set; }

    // UI State
    public bool ShowBoardPicker { get; set; }

    // Events
    public event Action? StateChanged;

    /// <summary>
    /// Lädt Order-Details
    /// </summary>
    public async Task LoadAsync(Guid orderId)
    {
        IsLoading = true;
        ErrorMessage = null;
        NotifyStateChanged();

        try
        {
            var query = new GetOrderByIdQuery(orderId);
            var result = await _mediator.Send(query);

            if (result.Success)
            {
                Order = result.GetOk(); // Direkt Domain-Entity
                ErrorMessage = null;
            }
            else
            {
                ErrorMessage = result.GetError().Message;
                Order = null;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Fehler beim Laden der Order: {ex.Message}";
            Order = null;
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Order absenden (Draft -> Submitted)
    /// </summary>
    public async Task SubmitOrderAsync()
    {
        if (Order == null || !CanSubmit(Order)) return;

        IsLoading = true;
        ErrorMessage = null;
        SuccessMessage = null;
        NotifyStateChanged();

        try
        {
            var updatedOrder = Order with { Status = OrderStatus.Submitted };
            var command = new CreateOrUpdateOrderCommand(updatedOrder);
            var result = await _mediator.Send(command);

            if (result.Success)
            {
                SuccessMessage = "Order erfolgreich eingereicht.";
                Order = Order with { Status = OrderStatus.Submitted };
            }
            else
            {
                ErrorMessage = result.GetError().Message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Fehler beim Absenden der Order: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Order stornieren
    /// </summary>
    public async Task CancelOrderAsync()
    {
        if (Order == null || !CanCancel(Order)) return;

        IsLoading = true;
        ErrorMessage = null;
        SuccessMessage = null;
        NotifyStateChanged();

        try
        {
            var updatedOrder = Order with { Status = OrderStatus.Cancelled };
            var command = new CreateOrUpdateOrderCommand(updatedOrder);
            var result = await _mediator.Send(command);

            if (result.Success)
            {
                SuccessMessage = "Order wurde storniert.";
                Order = Order with { Status = OrderStatus.Cancelled };
            }
            else
            {
                ErrorMessage = result.GetError().Message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Fehler beim Stornieren der Order: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Order löschen
    /// </summary>
    public async Task DeleteOrderAsync()
    {
        if (Order == null) return;

        IsLoading = true;
        ErrorMessage = null;
        NotifyStateChanged();

        try
        {
            var command = new DeleteOrderCommand(Order.Id);
            var result = await _mediator.Send(command);

            if (result.Success)
            {
                _navigationManager.NavigateTo("/orders");
            }
            else
            {
                ErrorMessage = result.GetError().Message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Fehler beim Löschen der Order: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Board zur Order hinzufügen
    /// </summary>
    public async Task AddBoardToOrderAsync(Guid boardId, long quantity)
    {
        if (Order == null) return;

        IsLoading = true;
        ErrorMessage = null;
        SuccessMessage = null;
        NotifyStateChanged();

        try
        {
            var command = new AddBoardToOrderCommand(Order.Id, boardId, quantity);
            var result = await _mediator.Send(command);

            if (result.Success)
            {
                SuccessMessage = "Board erfolgreich hinzugefügt.";
                // Reload order to get updated boards
                await LoadAsync(Order.Id);
            }
            else
            {
                ErrorMessage = result.GetError().Message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Fehler beim Hinzufügen des Boards: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Navigiert zur Board-Detail-Seite
    /// </summary>
    public void NavigateToBoard(Guid boardId)
    {
        _navigationManager.NavigateTo($"/boards/{boardId}");
    }

    public async Task RemoveBoardAsync(Guid boardId)
    {
        if (Order == null) return;

        IsLoading = true;
        ErrorMessage = null;
        SuccessMessage = null;
        NotifyStateChanged();

        try
        {
            var updated = Order.RemoveBoard(boardId);
            var command = new CreateOrUpdateOrderCommand(updated);
            var result = await _mediator.Send(command);
            if (result.Success)
            {
                SuccessMessage = "Board removed from order.";
                await LoadAsync(updated.Id);
            }
            else
            {
                ErrorMessage = result.GetError().Message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Fehler beim Entfernen des Boards: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Öffnet Board-Picker
    /// </summary>
    public void OpenBoardPicker()
    {
        ShowBoardPicker = true;
        NotifyStateChanged();
    }

    /// <summary>
    /// Schließt Board-Picker
    /// </summary>
    public void CloseBoardPicker()
    {
        ShowBoardPicker = false;
        NotifyStateChanged();
    }

    // UI-Helper Methods
    public string GetStatusDisplayText(Order order) => order.Status switch
    {
        OrderStatus.Draft => "📝 Entwurf",
        OrderStatus.Submitted => "📤 Eingereicht",
        OrderStatus.InProduction => "🔧 In Produktion",
        OrderStatus.Completed => "✅ Fertig",
        OrderStatus.Cancelled => "❌ Storniert",
        OrderStatus.Archived => "📦 Archiviert",
        _ => order.Status.ToString()
    };

    public bool CanSubmit(Order order) => order.Status == OrderStatus.Draft;
    public bool CanCancel(Order order) => order.Status is OrderStatus.Draft or OrderStatus.Submitted;
    public bool IsEditable(Order order) => order.Status == OrderStatus.Draft;
    public int GetBoardCount(Order order) => order.Boards.Count;
    public int GetTotalComponents(Order order) => order.Boards.Sum(b => b.Components.Count);
    public long GetQuantityForBoard(Guid boardId) =>
        Order?.BoardIds.FirstOrDefault(b => b.Id == boardId)?.Quantity ?? 0;

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }
}
