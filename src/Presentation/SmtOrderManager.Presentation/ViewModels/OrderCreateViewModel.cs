using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.AspNetCore.Components;
using SmtOrderManager.Application.Features.Orders.Commands.CreateOrUpdateOrder;
using SmtOrderManager.Domain.Entities;

namespace SmtOrderManager.Presentation.ViewModels;

public class OrderCreateViewModel
{
    private readonly IMediator _mediator;
    private readonly NavigationManager _navigationManager;

    public OrderCreateViewModel(IMediator mediator, NavigationManager navigationManager)
    {
        _mediator = mediator;
        _navigationManager = navigationManager;
    }

    [Required] public string Description { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; } = DateTime.Today;
    public Guid UserId { get; set; } = Guid.Parse("00000000-0000-0000-0000-000000000001"); // TODO: Get from ICurrentUserService

    public bool IsLoading { get; private set; }
    public string? ErrorMessage { get; private set; }

    public event Action? StateChanged;

    public async Task CreateAsync()
    {
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
            ErrorMessage = $"Fehler: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();
}
