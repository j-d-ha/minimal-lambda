using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace AwsLambda.Host.Builder;

/// <summary>
///     Overloads for <see cref="ILambdaOnShutdownBuilder.OnShutdown(LambdaShutdownDelegate)" />
///     that support automatic dependency injection for shutdown handlers.
/// </summary>
[ExcludeFromCodeCoverage]
public static class OnShutdownLambdaApplicationExtensions
{
    /// <summary>Registers a shutdown handler that will be run when the Lambda runtime shuts down.</summary>
    /// <remarks>
    ///     Source generation creates the wiring code to resolve handler dependencies, using
    ///     compile-time interceptors to replace the calls. Dependencies are scoped per handler. If a
    ///     CancellationToken is requested, it will be cancelled before the Lambda runtime forces shutdown.
    ///     The handler can be synchronous or asynchronous; async handlers are awaited before shutdown
    ///     completes.
    /// </remarks>
    /// <note>Shutdown logic should execute quickly as time is minimal before forced termination.</note>
    /// <param name="application">The Lambda application.</param>
    /// <param name="handler">An asynchronous handler function.</param>
    /// <returns>The current <see cref="ILambdaOnShutdownBuilder" /> instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if called at runtime; this exception is
    ///     unreachable as this method is intercepted by the source generator code at compile time.
    /// </exception>
    /// <seealso cref="LambdaShutdownDelegate" />
    /// <seealso cref="ILambdaOnShutdownBuilder.OnShutdown(LambdaShutdownDelegate)" />
    public static ILambdaOnShutdownBuilder OnShutdown(
        this ILambdaOnShutdownBuilder application,
        Delegate handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }
}
