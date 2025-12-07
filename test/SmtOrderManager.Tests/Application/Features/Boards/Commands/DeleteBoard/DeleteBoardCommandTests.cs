using Moq;
using SmtOrderManager.Application.Features.Boards.Commands.DeleteBoard;
using SmtOrderManager.Domain.Repositories;
using SmtOrderManager.Tests.Application.TestHelpers;

namespace SmtOrderManager.Tests.Application.Features.Boards.Commands.DeleteBoard;

public class DeleteBoardCommandTests
{
    [Fact]
    public async Task Handle_DeletesBoard()
    {
        var repoMock = new Mock<IBoardRepository>();
        repoMock.Setup(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok);

        var logger = TestLoggerFactory.CreateLogger<DeleteBoardCommandHandler>();
        var handler = new DeleteBoardCommandHandler(repoMock.Object, logger);

        var boardId = Guid.NewGuid();
        var result = await handler.Handle(new DeleteBoardCommand(boardId), CancellationToken.None);

        Assert.True(result.Success);
        repoMock.Verify(r => r.DeleteAsync(boardId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
