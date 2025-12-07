using Moq;
using SmtOrderManager.Application.Features.Components.Commands.CreateOrUpdateComponent;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;
using SmtOrderManager.Domain.Services;
using SmtOrderManager.Tests.Application.TestHelpers;

namespace SmtOrderManager.Tests.Application.Features.Components.Commands.CreateOrUpdateComponent;

public class CreateOrUpdateComponentCommandTests
{
    [Fact]
    public async Task Handle_UploadsImage_AndUpsertsComponent()
    {
        var component = Component.Create("C1", "desc", 1);
        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });

        var repoMock = new Mock<IComponentRepository>();
        repoMock.Setup(r => r.AddOrUpdateAsync(It.IsAny<Component>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok);

        var blobMock = new Mock<IBlobStorageService>();
        blobMock.Setup(b => b.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok);
        blobMock.Setup(b => b.GetBlobUrl(It.IsAny<string>()))
            .Returns("http://localhost/blob.png");

        var logger = TestLoggerFactory.CreateLogger<CreateOrUpdateComponentCommandHandler>();
        var handler = new CreateOrUpdateComponentCommandHandler(repoMock.Object, blobMock.Object, logger);

        var result = await handler.Handle(
            new CreateOrUpdateComponentCommand(component, stream, "image.png"),
            CancellationToken.None);

        Assert.True(result.Success);
        repoMock.Verify(r => r.AddOrUpdateAsync(
            It.Is<Component>(c => c.Id == component.Id && c.ImageUrl == "http://localhost/blob.png"),
            It.IsAny<CancellationToken>()), Times.Once);
        blobMock.Verify(b => b.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
