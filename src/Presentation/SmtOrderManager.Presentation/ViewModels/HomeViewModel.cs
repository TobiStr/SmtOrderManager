using MediatR;
using SmtOrderManager.Application.Features.Boards.Queries.GetAllBoards;
using SmtOrderManager.Application.Features.Components.Queries.GetAllComponents;
using SmtOrderManager.Application.Features.Orders.Queries.GetAllOrders;

namespace SmtOrderManager.Presentation.ViewModels;

public class HomeViewModel
{
    private readonly IMediator _mediator;

    public HomeViewModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public int TotalOrders { get; private set; }
    public int TotalBoards { get; private set; }
    public int TotalComponents { get; private set; }
    public bool IsLoading { get; private set; }
    public string? ErrorMessage { get; private set; }

    public event Action? StateChanged;

    public async Task LoadStatisticsAsync()
    {
        IsLoading = true;
        NotifyStateChanged();

        try
        {
            var ordersResult = await _mediator.Send(new GetAllOrdersQuery());
            var boardsResult = await _mediator.Send(new GetAllBoardsQuery());
            var componentsResult = await _mediator.Send(new GetAllComponentsQuery());

            if (ordersResult.Success) TotalOrders = ordersResult.GetOk().Count();
            if (boardsResult.Success) TotalBoards = boardsResult.GetOk().Count();
            if (componentsResult.Success) TotalComponents = componentsResult.GetOk().Count();
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

    private void NotifyStateChanged() => StateChanged?.Invoke();
}
