using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace SmtOrderManager.Tests.Infrastructure.CosmosDb;

internal static class CosmosTestHelpers
{
    public static ILoggerFactory CreateLoggerFactory() =>
        LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
        });

    public static ItemResponse<T> CreateItemResponse<T>(T resource, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var responseMock = new Moq.Mock<ItemResponse<T>>();
        responseMock.SetupGet(r => r.Resource).Returns(resource);
        responseMock.SetupGet(r => r.StatusCode).Returns(statusCode);
        responseMock.SetupGet(r => r.Headers).Returns(new Headers());
        return responseMock.Object;
    }

    public static FeedResponse<T> CreateFeedResponse<T>(IEnumerable<T> items, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var list = items.ToList();
        var responseMock = new Moq.Mock<FeedResponse<T>>();

        responseMock.As<IEnumerable<T>>().Setup(r => r.GetEnumerator()).Returns(() => list.GetEnumerator());
        responseMock.Setup(r => r.GetEnumerator()).Returns(() => list.GetEnumerator());
        responseMock.SetupGet(r => r.Count).Returns(list.Count);
        responseMock.SetupGet(r => r.StatusCode).Returns(statusCode);
        responseMock.SetupGet(r => r.Headers).Returns(new Headers());

        return responseMock.Object;
    }

    public static FeedIterator<T> CreateFeedIterator<T>(params FeedResponse<T>[] responses)
    {
        var queue = new Queue<FeedResponse<T>>(responses);
        return new StubFeedIterator<T>(queue);
    }

    private sealed class StubFeedIterator<T> : FeedIterator<T>
    {
        private readonly Queue<FeedResponse<T>> _responses;

        public StubFeedIterator(Queue<FeedResponse<T>> responses)
        {
            _responses = responses;
        }

        public override bool HasMoreResults => _responses.Count > 0;

        public override Task<FeedResponse<T>> ReadNextAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_responses.Dequeue());
        }
    }
}
