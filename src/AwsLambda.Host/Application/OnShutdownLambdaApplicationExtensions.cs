using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace AwsLambda.Host;

/// <summary>
///     Overloads for <see cref="ILambdaApplication.OnShutdown(LambdaShutdownDelegate)" /> that
///     support automatic dependency injection for shutdown handlers with zero to sixteen parameters.
/// </summary>
/// <remarks>
///     Source generation creates the wiring code to resolve handler dependencies, using
///     compile-time interceptors to replace the calls. Instead of using the base delegate, declare
///     handler parameters to be automatically injected from the dependency injection container. A
///     scope is created for each handler invocation, and the container is disposed of after the
///     handler returns.
/// </remarks>
/// <example>
///     <code>
///         lambda.OnShutdown(async (ILogger logger, DbContext database) =>
///         {
///             logger.LogInformation("Shutting down");
///             await database.FlushAsync();
///         });
///     </code>
/// </example>
[ExcludeFromCodeCoverage]
public static class OnShutdownLambdaApplicationExtensions
{
    /// <inheritdoc cref="OnShutdown(ILambdaApplication, Func{Task})" />
    /// <typeparam name="T1">The type of the first handler parameter.</typeparam>
    public static ILambdaApplication OnShutdown(
        this ILambdaApplication application,
        Delegate handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }
}
