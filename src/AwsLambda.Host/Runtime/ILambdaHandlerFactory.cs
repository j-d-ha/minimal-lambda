using Amazon.Lambda.Core;

namespace AwsLambda.Host.Runtime;

/// <summary>
///     Responsible for creating and composing the Lambda handler. Handles middleware pipeline
///     composition and request execution wrapping.
/// </summary>
internal interface ILambdaHandlerFactory
{
    /// <summary>
    ///     Creates a fully composed Lambda handler that includes middleware pipeline composition and
    ///     request processing (serialization, deserialization, context management). The handler,
    ///     middleware, and all state are obtained from dependency injection.
    /// </summary>
    /// <param name="stoppingToken">Cancellation token for service shutdown.</param>
    /// <returns>A handler function that accepts input stream and Lambda context.</returns>
    Func<Stream, ILambdaContext, Task<Stream>> CreateHandler(CancellationToken stoppingToken);
}
