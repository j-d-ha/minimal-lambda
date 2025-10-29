using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Microsoft.Extensions.Options;

namespace AwsLambda.Host;

/// <summary>
/// Adapts AWS Lambda bootstrap configuration and execution.
/// This class abstracts away AWS SDK complexity and bootstrap configuration details.
/// </summary>
internal sealed class LambdaBootstrapAdapter : ILambdaBootstrapOrchestrator
{
    private readonly LambdaHostSettings _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="LambdaBootstrapAdapter"/> class.
    /// </summary>
    /// <param name="lambdaHostSettings">The options containing Lambda host bootstrap configuration.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="lambdaHostSettings"/> is null.</exception>
    public LambdaBootstrapAdapter(IOptions<LambdaHostSettings> lambdaHostSettings)
    {
        ArgumentNullException.ThrowIfNull(lambdaHostSettings);

        _settings = lambdaHostSettings.Value;
    }

    /// <summary>
    /// Runs the Lambda bootstrap with the provided handler.
    /// </summary>
    /// <remarks>
    /// We cannot directly create a LambdaBootstrap with a custom HTTP client and settings
    /// because there is no public constructor that accepts both. This adapter works around
    /// that limitation by conditionally creating the bootstrap based on configuration.
    /// </remarks>
    /// <param name="handler">The processed handler function that accepts input stream and Lambda context.</param>
    /// <param name="stoppingToken">Cancellation token triggered when the service is shutting down.</param>
    /// <returns>A task representing the asynchronous bootstrap execution.</returns>
    public async Task RunAsync(
        Func<Stream, ILambdaContext, Task<Stream>> handler,
        CancellationToken stoppingToken
    )
    {
        // Wrap the handler with HandlerWrapper to match Lambda runtime expectations.
        using var wrappedHandler = HandlerWrapper.GetHandlerWrapper(handler);

        // Create the bootstrap based on configuration.
        using var bootstrap = _settings.BootstrapHttpClient is null
            ? new LambdaBootstrap(wrappedHandler, _settings.BootstrapOptions, null)
            : new LambdaBootstrap(
                _settings.BootstrapHttpClient,
                wrappedHandler,
                _settings.BootstrapOptions,
                null
            );

        await bootstrap.RunAsync(stoppingToken);
    }
}
