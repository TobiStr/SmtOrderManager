using Moq;
using SmtOrderManager.Application.Features.Components.Queries.GetComponentByName;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;
using SmtOrderManager.Tests.Application.TestHelpers;

namespace SmtOrderManager.Tests.Application.Features.Components.Queries.GetComponentByName;

public class GetComponentByNameQueryTests
{
    [Fact]
    public async Task Handle_ReturnsComponent()
    {
        var component = Component.Create("C1", "desc", 1, Guid.NewGuid());

        var repoMock = new Mock<IComponentRepository>();
        repoMock.Setup(r => r.GetByNameAsync(component.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(component);

        var logger = TestLoggerFactory.CreateLogger<GetComponentByNameQueryHandler>();
        var handler = new GetComponentByNameQueryHandler(repoMock.Object, logger);

        var result = await handler.Handle(new GetComponentByNameQuery(component.Name), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(component, result.GetOk());
        repoMock.Verify(r => r.GetByNameAsync(component.Name, It.IsAny<CancellationToken>()), Times.Once);
    }
}
