using Moq;
using SmtOrderManager.Application.Features.Components.Queries.GetAllComponents;
using SmtOrderManager.Domain.Entities;
using SmtOrderManager.Domain.Repositories;
using SmtOrderManager.Tests.Application.TestHelpers;

namespace SmtOrderManager.Tests.Application.Features.Components.Queries.GetAllComponents;

public class GetAllComponentsQueryTests
{
    [Fact]
    public async Task Handle_ReturnsAllComponents()
    {
        var components = new[] { Component.Create("C1", "desc", 1, Guid.NewGuid()) };

        var repoMock = new Mock<IComponentRepository>();
        repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IEnumerable<Component>>.Ok(components));

        var logger = TestLoggerFactory.CreateLogger<GetAllComponentsQueryHandler>();
        var handler = new GetAllComponentsQueryHandler(repoMock.Object, logger);

        var result = await handler.Handle(new GetAllComponentsQuery(), CancellationToken.None);

        Assert.True(result.Success);
        Assert.Single(result.GetOk());
        repoMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
