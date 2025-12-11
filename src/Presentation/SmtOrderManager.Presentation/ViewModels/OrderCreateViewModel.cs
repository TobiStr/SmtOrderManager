using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SmtOrderManager.Application.Features.Orders.Commands.CreateOrUpdateOrder;
using SmtOrderManager.Domain.Entities;

namespace SmtOrderManager.Presentation.ViewModels;

public class OrderCreateViewModel
{
    private readonly IMediator _mediator;
    private readonly NavigationManager _navigationManager;
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public OrderCreateViewModel(
        IMediator mediator,
        NavigationManager navigationManager,
        AuthenticationStateProvider authenticationStateProvider)
    {
        _mediator = mediator;
        _navigationManager = navigationManager;
        _authenticationStateProvider = authenticationStateProvider;
    }

    [Required] public string Description { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; } = DateTime.Today;
    public Guid UserId { get; private set; }

    public bool IsLoading { get; private set; }
    public string? ErrorMessage { get; private set; }

    public event Action? StateChanged;

    public async Task CreateAsync()
    {
        await EnsureUserIdAsync();
        IsLoading = true;
        ErrorMessage = null;
        NotifyStateChanged();

        try
        {
            var order = Order.Create(Description, OrderDate, UserId);
            var command = new CreateOrUpdateOrderCommand(order);
            var result = await _mediator.Send(command);

            if (result.Success)
            {
                var createdOrder = result.GetOk();
                _navigationManager.NavigateTo($"/orders/{createdOrder.Id}");
            }
            else
            {
                ErrorMessage = result.GetError().Message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    private async Task EnsureUserIdAsync()
    {
        if (UserId != Guid.Empty)
        {
            return;
        }

        var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var principal = state.User;
        var idValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(idValue, out var userId))
        {
            UserId = userId;
        }
        else
        {
            throw new InvalidOperationException("Cannot determine current user ID for order creation.");
        }
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();
}
