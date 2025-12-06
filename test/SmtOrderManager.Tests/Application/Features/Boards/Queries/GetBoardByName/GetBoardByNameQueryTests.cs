using Moq;
using SmtOrderManager.Application.Features.Boards.Queries.GetBoardByName;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;
using SmtOrderManager.Tests.Application.TestHelpers;

namespace SmtOrderManager.Tests.Application.Features.Boards.Queries.GetBoardByName;

public class GetBoardByNameQueryTests
{
    [Fact]
    public async Task Handle_ReturnsBoard()
    {
        var board = Board.Create("B1", "desc", 10, 5, Guid.NewGuid());

        var repoMock = new Mock<IBoardRepository>();
        repoMock.Setup(r => r.GetByNameAsync(board.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(board);

        var logger = TestLoggerFactory.CreateLogger<GetBoardByNameQueryHandler>();
        var handler = new GetBoardByNameQueryHandler(repoMock.Object, logger);

        var result = await handler.Handle(new GetBoardByNameQuery(board.Name), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(board, result.GetOk());
        repoMock.Verify(r => r.GetByNameAsync(board.Name, It.IsAny<CancellationToken>()), Times.Once);
    }
}
