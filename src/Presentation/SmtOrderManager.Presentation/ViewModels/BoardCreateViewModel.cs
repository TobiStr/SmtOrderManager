using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.AspNetCore.Components;
using SmtOrderManager.Application.Features.Boards.Commands.CreateOrUpdateBoard;
using SmtOrderManager.Domain.Entities;

namespace SmtOrderManager.Presentation.ViewModels;

public class BoardCreateViewModel
{
    private readonly IMediator _mediator;
    private readonly NavigationManager _navigationManager;

    public BoardCreateViewModel(IMediator mediator, NavigationManager navigationManager)
    {
        _mediator = mediator;
        _navigationManager = navigationManager;
    }

    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string Description { get; set; } = string.Empty;
    [Required] [Range(1, 10000)] public decimal Length { get; set; } = 100;
    [Required] [Range(1, 10000)] public decimal Width { get; set; } = 100;
    public Guid? OrderId { get; set; }

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
            var board = Board.Create(Name, Description, Length, Width);
            var command = new CreateOrUpdateBoardCommand(board);
            var result = await _mediator.Send(command);

            if (result.Success)
            {
                var createdBoard = result.GetOk();
                _navigationManager.NavigateTo($"/boards/{createdBoard.Id}");
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
