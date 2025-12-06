using Moq;
using SmtOrderManager.Application.Features.Boards.Queries.GetAllBoards;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;
using SmtOrderManager.Tests.Application.TestHelpers;

namespace SmtOrderManager.Tests.Application.Features.Boards.Queries.GetAllBoards;

public class GetAllBoardsQueryTests
{
    [Fact]
    public async Task Handle_ReturnsAllBoards()
    {
        var boards = new[] { Board.Create("B1", "desc", 10, 5, Guid.NewGuid()) };

        var repoMock = new Mock<IBoardRepository>();
        repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<Board>>.Ok(boards));

        var logger = TestLoggerFactory.CreateLogger<GetAllBoardsQueryHandler>();
        var handler = new GetAllBoardsQueryHandler(repoMock.Object, logger);

        var result = await handler.Handle(new GetAllBoardsQuery(), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Single(result.GetOk());
        repoMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
