using Amazon.Lambda.Core;

namespace AwsLambda.Host;

/// <summary>
/// Responsible for orchestrating the AWS Lambda bootstrap.
/// Abstracts away AWS SDK complexity and bootstrap configuration.
/// </summary>
internal interface ILambdaBootstrapOrchestrator
{
    /// <summary>
    /// Runs the Lambda bootstrap with the provided handler.
    /// </summary>
    /// <param name="handler">The processed handler function.</param>
    /// <param name="stoppingToken">Cancellation token for service shutdown.</param>
    /// <returns>A task representing the bootstrap execution.</returns>
    Task RunAsync(
        Func<Stream, ILambdaContext, Task<Stream>> handler,
        CancellationToken stoppingToken
    );
}
