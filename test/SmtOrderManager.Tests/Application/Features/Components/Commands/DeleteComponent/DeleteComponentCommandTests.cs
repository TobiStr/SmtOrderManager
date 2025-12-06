using Moq;
using SmtOrderManager.Application.Features.Components.Commands.DeleteComponent;
using SmtOrderManager.Domain.Repositories;
using SmtOrderManager.Tests.Application.TestHelpers;

namespace SmtOrderManager.Tests.Application.Features.Components.Commands.DeleteComponent;

public class DeleteComponentCommandTests
{
    [Fact]
    public async Task Handle_DeletesComponent()
    {
        var repoMock = new Mock<IComponentRepository>();
        repoMock.Setup(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok);

        var logger = TestLoggerFactory.CreateLogger<DeleteComponentCommandHandler>();
        var handler = new DeleteComponentCommandHandler(repoMock.Object, logger);

        var componentId = Guid.NewGuid();
        var result = await handler.Handle(new DeleteComponentCommand(componentId), CancellationToken.None);

        Assert.True(result.Success);
        repoMock.Verify(r => r.DeleteAsync(componentId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
