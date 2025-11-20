namespace AwsLambda.Host;

/// <summary>Builder for composing Lambda init handlers that execute during the Init phase.</summary>
/// <remarks>
///     <para>
///         Register handlers using <see cref="OnInit" /> and call <see cref="Build" /> to create a
///         composed <see cref="LambdaInitDelegate" /> that executes all handlers concurrently.
///     </para>
///     <para>
///         All handlers execute concurrently with a configurable timeout. If any handler throws an
///         exception, errors are collected and rethrown as an <see cref="AggregateException" /> after
///         all handlers complete. If any handler returns <c>false</c>, the Lambda Init phase aborts
///         after all concurrent handlers complete.
///     </para>
/// </remarks>
public interface ILambdaOnInitBuilder
{
    /// <summary>Gets the read-only list of registered Init handlers.</summary>
    IReadOnlyList<LambdaInitDelegate> InitHandlers { get; }

    /// <summary>Gets the service provider for dependency injection.</summary>
    IServiceProvider Services { get; }

    /// <summary>Registers a handler to execute during the Lambda Init phase.</summary>
    /// <param name="handler">The <see cref="LambdaInitDelegate" /> to register.</param>
    /// <returns>The current <see cref="ILambdaOnInitBuilder" /> instance for method chaining.</returns>
    ILambdaOnInitBuilder OnInit(LambdaInitDelegate handler);

    /// <summary>Builds the final init delegate by composing all registered handlers.</summary>
    /// <remarks>
    ///     <para>
    ///         Composes all registered handlers into a single function that executes them concurrently
    ///         during the Init phase with timeout enforcement and error aggregation. The returned function
    ///         accepts a <see cref="CancellationToken" /> for cancellation support and can be invoked
    ///         multiple times.
    ///     </para>
    /// </remarks>
    /// <returns>
    ///     A function that accepts a <see cref="CancellationToken" /> and executes all registered
    ///     handlers concurrently. Ready for the Lambda Init phase.
    /// </returns>
    Func<CancellationToken, Task<bool>> Build();
}
