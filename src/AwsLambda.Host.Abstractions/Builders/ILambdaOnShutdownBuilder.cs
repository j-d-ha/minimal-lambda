namespace AwsLambda.Host;

/// <summary>Builder for composing Lambda shutdown handlers that execute during the Shutdown phase.</summary>
/// <remarks>
///     <para>
///         Register handlers using <see cref="OnInit" /> and call <see cref="Build" /> to create a
///         composed <see cref="LambdaShutdownDelegate" /> that executes all handlers sequentially.
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

    /// <summary>Gets the read-only list of registered shutdown handlers.</summary>
    IReadOnlyList<LambdaShutdownDelegate> ShutdownHandlers { get; }

    /// <summary>Registers a handler to execute during the Lambda Shutdown phase.</summary>
    /// <param name="handler">The <see cref="LambdaShutdownDelegate" /> to register.</param>
    /// <returns>The current <see cref="ILambdaOnShutdownBuilder" /> instance for method chaining.</returns>
    ILambdaOnShutdownBuilder OnInit(LambdaShutdownDelegate handler);

    /// <summary>Builds the final shutdown delegate by composing all registered handlers.</summary>
    /// <remarks>
    ///     <para>
    ///         All registered handlers are composed into a single <see cref="LambdaShutdownDelegate" />
    ///         that executes them sequentially during the Shutdown phase, with timeout enforcement and
    ///         exception handling.
    ///     </para>
    /// </remarks>
    /// <returns>A composed <see cref="LambdaShutdownDelegate" /> ready for the Lambda Shutdown phase.</returns>
    LambdaShutdownDelegate Build();
}
