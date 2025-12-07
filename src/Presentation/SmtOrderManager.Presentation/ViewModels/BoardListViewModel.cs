using MediatR;
using Microsoft.AspNetCore.Components;
using SmtOrderManager.Application.Features.Boards.Queries.GetAllBoards;
using SmtOrderManager.Domain.Entities;

namespace SmtOrderManager.Presentation.ViewModels;

public class BoardListViewModel
{
    private readonly IMediator _mediator;
    private readonly NavigationManager _navigationManager;

    public BoardListViewModel(IMediator mediator, NavigationManager navigationManager)
    {
        _mediator = mediator;
        _navigationManager = navigationManager;
    }

    public List<Board> Boards { get; private set; } = new();
    public List<Board> FilteredBoards => ApplyFilter();
    public string? SearchTerm { get; set; }
    public bool IsLoading { get; private set; }
    public string? ErrorMessage { get; private set; }

    public event Action? StateChanged;

    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        NotifyStateChanged();

        try
        {
            var query = new GetAllBoardsQuery();
            var result = await _mediator.Send(query);

            if (result.Success)
            {
                Boards = result.GetOk().ToList();
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

    public void NavigateToDetail(Guid boardId) => _navigationManager.NavigateTo($"/boards/{boardId}");
    public void NavigateToCreate() => _navigationManager.NavigateTo("/boards/create");

    // UI-Helper Methods
    public string GetDimensions(Board board) => $"{board.Length} × {board.Width} mm";
    public int GetComponentCount(Board board) => board.Components.Count;
    public string GetSummary(Board board) => $"{board.Name} ({GetComponentCount(board)} Components)";

    private List<Board> ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(SearchTerm)) return Boards;
        var search = SearchTerm.ToLowerInvariant();
        return Boards.Where(b => b.Name.ToLowerInvariant().Contains(search)).ToList();
    }

    private void NotifyStateChanged() => StateChanged?.Invoke();
}
