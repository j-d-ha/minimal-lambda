using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Microsoft.Extensions.Options;

namespace AwsLambda.Host;

/// <summary>
///     Adapts AWS Lambda bootstrap configuration and execution. This class abstracts away AWS SDK
///     complexity and bootstrap configuration details.
/// </summary>
internal sealed class LambdaBootstrapAdapter : ILambdaBootstrapOrchestrator
{
    private readonly LambdaHostOptions _settings;

    /// <summary>Initializes a new instance of the <see cref="LambdaBootstrapAdapter" /> class.</summary>
    /// <param name="lambdaHostSettings">The options containing Lambda host bootstrap configuration.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="lambdaHostSettings" /> is null.</exception>
    public LambdaBootstrapAdapter(IOptions<LambdaHostOptions> lambdaHostSettings)
    {
        ArgumentNullException.ThrowIfNull(lambdaHostSettings);

        _settings = lambdaHostSettings.Value;
    }

    /// <inheritdoc />
    public async Task RunAsync(
        Func<Stream, ILambdaContext, Task<Stream>> handler,
        LambdaBootstrapInitializer? initializer,
        CancellationToken stoppingToken
    )
    {
        // Wrap the handler with HandlerWrapper to match Lambda runtime expectations.
        using var wrappedHandler = HandlerWrapper.GetHandlerWrapper(handler);

        // Create the bootstrap based on configuration.
        using var bootstrap = _settings.BootstrapHttpClient is null
            ? new LambdaBootstrap(wrappedHandler, _settings.BootstrapOptions, initializer)
            : new LambdaBootstrap(
                _settings.BootstrapHttpClient,
                wrappedHandler,
                _settings.BootstrapOptions,
                initializer
            );

        await bootstrap.RunAsync(stoppingToken);
    }
}
