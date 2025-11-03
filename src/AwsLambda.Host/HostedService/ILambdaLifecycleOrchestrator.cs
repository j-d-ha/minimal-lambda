using Amazon.Lambda.RuntimeSupport;

namespace AwsLambda.Host;

/// <summary>
/// Orchestrates the Lambda lifecycle shutdown process, managing the execution of shutdown handlers
/// and collecting any exceptions that occur during the shutdown sequence.
/// </summary>
internal interface ILambdaLifecycleOrchestrator
{
    /// <summary>
    ///     Returns a Lambda bootstrap initializer that executes all registered initialization
    ///     callbacks at cold start, running them asynchronously and determining whether execution should
    ///     continue based on the collected results.
    /// </summary>
    /// <param name="cancellationToken">
    ///     A cancellation token to signal initialization handlers to stop
    ///     processing.
    /// </param>
    /// <returns>
    ///     A Lambda bootstrap initializer callback that executes the initialization callbacks and
    ///     returns a boolean indicating whether execution should continue.
    /// </returns>
    LambdaBootstrapInitializer OnInit(CancellationToken cancellationToken);

    /// <summary>
    /// Executes the shutdown lifecycle, running all registered shutdown handlers asynchronously
    /// and collecting any exceptions that occur.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to signal shutdown handlers to stop processing.</param>
    /// <returns>A task representing the asynchronous shutdown operation that returns any exceptions thrown by shutdown handlers.</returns>
    Task<IEnumerable<Exception>> OnShutdown(CancellationToken cancellationToken);
}
