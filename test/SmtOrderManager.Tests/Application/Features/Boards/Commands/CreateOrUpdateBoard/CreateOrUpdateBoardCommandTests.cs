using Moq;
using SmtOrderManager.Application.Features.Boards.Commands.CreateOrUpdateBoard;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;
using SmtOrderManager.Tests.Application.TestHelpers;

namespace SmtOrderManager.Tests.Application.Features.Boards.Commands.CreateOrUpdateBoard;

public class CreateOrUpdateBoardCommandTests
{
    [Fact]
    public async Task Handle_UpsertsBoard()
    {
        var board = Board.Create("B1", "desc", 10, 5, Guid.NewGuid());

        var repoMock = new Mock<IBoardRepository>();
        repoMock.Setup(r => r.AddOrUpdateAsync(board, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok);

        var logger = TestLoggerFactory.CreateLogger<CreateOrUpdateBoardCommandHandler>();
        var handler = new CreateOrUpdateBoardCommandHandler(repoMock.Object, logger);

        var result = await handler.Handle(new CreateOrUpdateBoardCommand(board), CancellationToken.None);

        Assert.True(result.Success);
        repoMock.Verify(r => r.AddOrUpdateAsync(board, It.IsAny<CancellationToken>()), Times.Once);
    }
}
