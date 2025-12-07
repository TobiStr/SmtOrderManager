using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmtOrderManager.Presentation;

namespace SmtOrderManager.Tests.Integration;

internal static class IntegrationTestSetup
{
    public static IServiceProvider CreateServiceProvider()
    {
        var builder = DependencyInjection.CreateBuilder(Array.Empty<string>());

        return builder.Services.BuildServiceProvider();
    }
}
