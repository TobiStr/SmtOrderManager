using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SmtOrderManager.Application.Features.Orders.Queries.GetAllOrders;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Enums;

namespace SmtOrderManager.Presentation.ViewModels;

/// <summary>
/// ViewModel for order overview
/// </summary>
public class OrderListViewModel
{
    private readonly IMediator _mediator;
    private readonly NavigationManager _navigationManager;
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public OrderListViewModel(
        IMediator mediator,
        NavigationManager navigationManager,
        AuthenticationStateProvider authenticationStateProvider)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
        _authenticationStateProvider = authenticationStateProvider ?? throw new ArgumentNullException(nameof(authenticationStateProvider));
    }

    // Properties - Direkt Domain-Entities
    public List<Order> AllOrders { get; private set; } = new();
    public List<Order> FilteredOrders => ApplyFilters();
    public bool IsLoading { get; private set; }
    public string? ErrorMessage { get; private set; }

    // Filter Properties
    public string? SearchTerm { get; set; }
    public OrderStatus? SelectedStatus { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    // Statistics
    public int TotalCount => AllOrders.Count;
    public int FilteredCount => FilteredOrders.Count;
    public int DraftCount => AllOrders.Count(o => o.Status == OrderStatus.Draft);
    public int SubmittedCount => AllOrders.Count(o => o.Status == OrderStatus.Submitted);
    public int InProductionCount => AllOrders.Count(o => o.Status == OrderStatus.InProduction);
    public int CompletedCount => AllOrders.Count(o => o.Status == OrderStatus.Completed);

    // Events
    public event Action? StateChanged;

    /// <summary>
    /// Loads all orders
    /// </summary>
    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        NotifyStateChanged();

        try
        {
            var userId = await GetCurrentUserIdAsync();
            var query = new GetAllOrdersQuery(userId);
            var result = await _mediator.Send(query);

            if (result.Success)
            {
                var orders = result.GetOk();
                AllOrders = orders.ToList(); // Direct Domain-Entities
                ErrorMessage = null;
            }
            else
            {
                ErrorMessage = result.GetError().Message;
                AllOrders = new List<Order>();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading orders: {ex.Message}";
            AllOrders = new List<Order>();
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Applies filters
    /// </summary>
    public void ApplyFilter()
    {
        NotifyStateChanged();
    }

    /// <summary>
    /// Resets all filters
    /// </summary>
    public void ClearFilters()
    {
        SearchTerm = null;
        SelectedStatus = null;
        FromDate = null;
        ToDate = null;
        NotifyStateChanged();
    }

    /// <summary>
    /// Navigates to order detail page
    /// </summary>
    public void NavigateToDetail(Guid orderId)
    {
        _navigationManager.NavigateTo($"/orders/{orderId}");
    }

    /// <summary>
    /// Navigates to order create page
    /// </summary>
    public void NavigateToCreate()
    {
        _navigationManager.NavigateTo("/orders/create");
    }

    // UI-Helper Methods (instead of DTO-Properties)
    public string GetStatusDisplayText(Order order) => order.Status switch
    {
        OrderStatus.Draft => "📝 Draft",
        OrderStatus.Submitted => "📤 Submitted",
        OrderStatus.InProduction => "🔧 In Production",
        OrderStatus.Completed => "✅ Completed",
        OrderStatus.Cancelled => "❌ Cancelled",
        OrderStatus.Archived => "📦 Archived",
        _ => order.Status.ToString()
    };

    public bool CanSubmit(Order order) => order.Status == OrderStatus.Draft;
    public bool CanCancel(Order order) => order.Status is OrderStatus.Draft or OrderStatus.Submitted;
    public bool IsEditable(Order order) => order.Status == OrderStatus.Draft;
    public int GetBoardCount(Order order) => order.Boards.Count;

    private List<Order> ApplyFilters()
    {
        var filtered = AllOrders.AsEnumerable();

        // Text-Suche
        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            var searchLower = SearchTerm.ToLowerInvariant();
            filtered = filtered.Where(o =>
                o.Description.ToLowerInvariant().Contains(searchLower)
            );
        }

        // Status-Filter
        if (SelectedStatus.HasValue)
        {
            filtered = filtered.Where(o => o.Status == SelectedStatus.Value);
        }

        // Datums-Filter
        if (FromDate.HasValue)
        {
            filtered = filtered.Where(o => o.OrderDate >= FromDate.Value);
        }

        if (ToDate.HasValue)
        {
            filtered = filtered.Where(o => o.OrderDate <= ToDate.Value);
        }

        return filtered.OrderByDescending(o => o.OrderDate).ToList();
    }

    private void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }

    private async Task<Guid?> GetCurrentUserIdAsync()
    {
        var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var principal = state.User;
        var idValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(idValue, out var userId) ? userId : null;
    }
}
