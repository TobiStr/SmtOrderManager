using Moq;
using SmtOrderManager.Application.Features.Boards.Commands.AddComponentToBoard;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;
using SmtOrderManager.Tests.Application.TestHelpers;

namespace SmtOrderManager.Tests.Application.Features.Boards.Commands.AddComponentToBoard;

public class AddComponentToBoardCommandTests
{
    [Fact]
    public async Task Handle_AppendsComponentId_AndUpsertsBoard()
    {
        var componentId = Guid.NewGuid();
        var board = Board.Create("B1", "desc", 10, 5);
        var repoMock = new Mock<IBoardRepository>();
        repoMock.Setup(r => r.GetByIdAsync(board.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(board);
        repoMock.Setup(r => r.AddOrUpdateAsync(It.IsAny<Board>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok);

        var logger = TestLoggerFactory.CreateLogger<AddComponentToBoardCommandHandler>();
        var handler = new AddComponentToBoardCommandHandler(repoMock.Object, logger);

        var result = await handler.Handle(new AddComponentToBoardCommand(board.Id, componentId), CancellationToken.None);

        Assert.True(result.Success);
        repoMock.Verify(r => r.GetByIdAsync(board.Id, It.IsAny<CancellationToken>()), Times.Once);
        repoMock.Verify(r => r.AddOrUpdateAsync(
            It.Is<Board>(b => b.ComponentIds.Contains(componentId)),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
