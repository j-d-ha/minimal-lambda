using System.Collections.Concurrent;

namespace MinimalLambda.Builder;

/// <summary>Builder for composing Lambda shutdown handlers that execute during the Shutdown phase.</summary>
/// <remarks>
///     <para>
///         Register handlers using <see cref="OnShutdown" /> and call <see cref="Build" /> to create
///         a composed <see cref="LambdaShutdownDelegate" /> that executes all handlers sequentially.
///     </para>
///     <para>
///         All handlers execute sequentially during the Lambda Shutdown phase with a configurable
///         timeout. If any handler throws an exception, it is logged and execution continues with the
///         next handler. All handlers run even if another handlers fail.
///     </para>
/// </remarks>
public interface ILambdaOnShutdownBuilder
{
    /// <summary>Gets the service provider for dependency injection.</summary>
    IServiceProvider Services { get; }

    /// <summary>Gets a dictionary for storing state that is shared between handlers.</summary>
    ConcurrentDictionary<string, object?> Properties { get; }

    /// <summary>Gets the read-only list of registered shutdown handlers.</summary>
    IReadOnlyList<LambdaShutdownDelegate> ShutdownHandlers { get; }

    /// <summary>Registers a handler to execute during the Lambda Shutdown phase.</summary>
    /// <param name="handler">The <see cref="LambdaShutdownDelegate" /> to register.</param>
    /// <returns>The current <see cref="ILambdaOnShutdownBuilder" /> instance for method chaining.</returns>
    ILambdaOnShutdownBuilder OnShutdown(LambdaShutdownDelegate handler);

    /// <summary>Builds the final shutdown delegate by composing all registered handlers.</summary>
    /// <remarks>
    ///     <para>
    ///         Composes all registered handlers into a single function that executes them sequentially
    ///         during the Shutdown phase with timeout enforcement and exception handling. The returned
    ///         function accepts a <see cref="CancellationToken" /> for cancellation support and can be
    ///         invoked multiple times.
    ///     </para>
    /// </remarks>
    /// <returns>
    ///     A function that accepts a <see cref="CancellationToken" /> and executes all registered
    ///     handlers sequentially. Ready for the Lambda Shutdown phase.
    /// </returns>
    Func<CancellationToken, Task> Build();
}
