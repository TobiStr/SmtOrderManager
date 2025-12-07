using Moq;
using SmtOrderManager.Application.Features.Boards.Queries.GetBoardById;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;
using SmtOrderManager.Tests.Application.TestHelpers;

namespace SmtOrderManager.Tests.Application.Features.Boards.Queries.GetBoardById;

public class GetBoardByIdQueryTests
{
    [Fact]
    public async Task Handle_ReturnsBoard()
    {
        var board = Board.Create("B1", "desc", 10, 5);

        var repoMock = new Mock<IBoardRepository>();
        repoMock.Setup(r => r.GetByIdAsync(board.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(board);

        var logger = TestLoggerFactory.CreateLogger<GetBoardByIdQueryHandler>();
        var handler = new GetBoardByIdQueryHandler(repoMock.Object, logger);

        var result = await handler.Handle(new GetBoardByIdQuery(board.Id), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(board, result.GetOk());
        repoMock.Verify(r => r.GetByIdAsync(board.Id, It.IsAny<CancellationToken>()), Times.Once);
    }
}
