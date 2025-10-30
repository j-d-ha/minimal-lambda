namespace AwsLambda.Host;

/// <summary>
/// Orchestrates the Lambda lifecycle shutdown process, managing the execution of shutdown handlers
/// and collecting any exceptions that occur during the shutdown sequence.
/// </summary>
internal interface ILambdaLifecycleOrchestrator
{
    /// <summary>
    /// Executes the shutdown lifecycle, running all registered shutdown handlers concurrently
    /// and collecting any exceptions that occur.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to signal shutdown handlers to stop processing.</param>
    /// <returns>A task representing the asynchronous shutdown operation that returns any exceptions thrown by shutdown handlers.</returns>
    Task<IEnumerable<Exception>> OnShutdown(CancellationToken cancellationToken);
}
