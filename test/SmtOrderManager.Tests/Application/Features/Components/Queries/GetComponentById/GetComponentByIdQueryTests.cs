using Moq;
using SmtOrderManager.Application.Features.Components.Queries.GetComponentById;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;
using SmtOrderManager.Tests.Application.TestHelpers;

namespace SmtOrderManager.Tests.Application.Features.Components.Queries.GetComponentById;

public class GetComponentByIdQueryTests
{
    [Fact]
    public async Task Handle_ReturnsComponent()
    {
        var component = Component.Create("C1", "desc", 1, Guid.NewGuid());

        var repoMock = new Mock<IComponentRepository>();
        repoMock.Setup(r => r.GetByIdAsync(component.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(component);

        var logger = TestLoggerFactory.CreateLogger<GetComponentByIdQueryHandler>();
        var handler = new GetComponentByIdQueryHandler(repoMock.Object, logger);

        var result = await handler.Handle(new GetComponentByIdQuery(component.Id), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(component, result.GetOk());
        repoMock.Verify(r => r.GetByIdAsync(component.Id, It.IsAny<CancellationToken>()), Times.Once);
    }
}
