using Amazon.Lambda.Core;

namespace AwsLambda.Host.Runtime;

/// <summary>
///     Responsible for orchestrating the AWS Lambda bootstrap. Abstracts away AWS SDK complexity
///     and bootstrap configuration.
/// </summary>
internal interface ILambdaBootstrapOrchestrator
{
    /// <summary>Runs the Lambda bootstrap with the provided handler.</summary>
    /// <remarks>
    ///     This method starts and manages the AWS Lambda bootstrap loop, which continuously polls the
    ///     Lambda runtime API for invocation events. The handler processes each invocation and returns the
    ///     response. The bootstrap continues until the service is stopped or a fatal error occurs.
    /// </remarks>
    /// <param name="handler">The processed handler function that accepts input stream and Lambda context.</param>
    /// <param name="initializer">
    ///     Optional initialization handler invoked during the Function Init phase
    ///     for configuration and initialization tasks. Returns a boolean indicating whether initialization
    ///     should continue.
    /// </param>
    /// <param name="stoppingToken">Cancellation token triggered when the service is shutting down.</param>
    /// <returns>A task representing the asynchronous bootstrap execution.</returns>
    Task RunAsync(
        Func<Stream, ILambdaContext, Task<Stream>> handler,
        Func<CancellationToken, Task<bool>>? initializer,
        CancellationToken stoppingToken
    );
}
