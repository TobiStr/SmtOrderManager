using MediatR;
using Microsoft.Extensions.Logging;

namespace SmtOrderManager.Application.Behaviors;

public class UnhandledExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> _logger;

    public UnhandledExceptionBehavior(ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for request {RequestName}", typeof(TRequest).Name);

            // If response is Result or Result<T>, return error Result instead of throwing
            if (typeof(TResponse) == typeof(Result))
            {
                return (TResponse)(object)Result.Error(ex);
            }

            var resultGenericType = typeof(TResponse).IsGenericType
                && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>);

            if (resultGenericType)
            {
                var errorResult = Activator.CreateInstance(typeof(TResponse), ex);
                if (errorResult is not null)
                {
                    return (TResponse)errorResult;
                }
            }

            throw;
        }
    }
}
