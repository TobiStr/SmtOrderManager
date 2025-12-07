using System.Diagnostics;
using Xunit;

namespace SmtOrderManager.Tests.Integration;

/// <summary>
/// Fact attribute that only runs when integration tests are explicitly allowed.
/// Allows execution when a debugger is attached or when the enabling environment variable is set.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class IntegrationFactAttribute : FactAttribute
{
    private const string EnvVarName = "SMT_RUN_INTEGRATION_TESTS";

    public IntegrationFactAttribute()
    {
        if (IsAllowed())
        {
            return;
        }

        Skip = $"Integration test skipped. Attach a debugger or set {EnvVarName}=true to run.";
    }

    private static bool IsAllowed()
    {
        if (Debugger.IsAttached)
        {
            return true;
        }

        var value = Environment.GetEnvironmentVariable(EnvVarName);
        return !string.IsNullOrWhiteSpace(value)
               && value.Equals("true", StringComparison.OrdinalIgnoreCase)
               || value == "1";
    }
}
