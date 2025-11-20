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

    public LambdaBootstrapAdapter(IOptions<LambdaHostOptions> lambdaHostSettings)
    {
        ArgumentNullException.ThrowIfNull(lambdaHostSettings);

        _settings = lambdaHostSettings.Value;
    }

    /// <inheritdoc />
    public async Task RunAsync(
        Func<Stream, ILambdaContext, Task<Stream>> handler,
        Func<CancellationToken, Task<bool>>? initializer,
        CancellationToken stoppingToken
    )
    {
        var convertedInitializer = LambdaBootstrapInitializerAdapter(initializer, stoppingToken);

        // Wrap the handler with HandlerWrapper to match Lambda runtime expectations.
        using var wrappedHandler = HandlerWrapper.GetHandlerWrapper(handler);

        // Create the bootstrap based on configuration.
        using var bootstrap = _settings.BootstrapHttpClient is null
            ? new LambdaBootstrap(wrappedHandler, _settings.BootstrapOptions, convertedInitializer)
            : new LambdaBootstrap(
                _settings.BootstrapHttpClient,
                wrappedHandler,
                _settings.BootstrapOptions,
                convertedInitializer
            );

        await bootstrap.RunAsync(stoppingToken);
    }

    private static LambdaBootstrapInitializer LambdaBootstrapInitializerAdapter(
        Func<CancellationToken, Task<bool>>? handler,
        CancellationToken stoppingToken
    ) => () => handler?.Invoke(stoppingToken) ?? Task.FromResult(true);
}
