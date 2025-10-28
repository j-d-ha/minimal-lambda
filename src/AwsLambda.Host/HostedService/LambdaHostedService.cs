using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace AwsLambda.Host;

internal sealed class LambdaHostedService : BackgroundService
{
    private readonly ILambdaCancellationTokenSourceFactory _cancellationTokenSourceFactory;
    private readonly DelegateHolder _delegateHolder;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly LambdaHostSettings _settings;

    public LambdaHostedService(
        IOptions<LambdaHostSettings> lambdaHostSettings,
        DelegateHolder delegateHolder,
        ILambdaCancellationTokenSourceFactory lambdaCancellationTokenSourceFactory,
        IServiceScopeFactory serviceScopeFactory,
        IHostApplicationLifetime lifetime
    )
    {
        ArgumentNullException.ThrowIfNull(lambdaHostSettings);
        ArgumentNullException.ThrowIfNull(delegateHolder);
        ArgumentNullException.ThrowIfNull(lambdaCancellationTokenSourceFactory);
        ArgumentNullException.ThrowIfNull(serviceScopeFactory);
        ArgumentNullException.ThrowIfNull(lifetime);

        if (!delegateHolder.IsHandlerSet)
            throw new InvalidOperationException("Lambda Handler is not set");

        _settings = lambdaHostSettings.Value;
        _delegateHolder = delegateHolder;
        _cancellationTokenSourceFactory = lambdaCancellationTokenSourceFactory;
        _scopeFactory = serviceScopeFactory;

        lifetime.ApplicationStopped.Register(
            state => RunShutdownHandlers((IServiceScopeFactory)state!),
            _scopeFactory
        );
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Build the middleware pipeline and wrap the handler.
        var handler = BuildMiddlewarePipeline(
            _delegateHolder.Middlewares,
            _delegateHolder.Handler!
        );

        var wrappedHandler = HandlerWrapper.GetHandlerWrapper(GetHandler(handler, stoppingToken));

        var bootstrap = _settings.BootstrapHttpClient is null
            ? new LambdaBootstrap(wrappedHandler, _settings.BootstrapOptions, null)
            : new LambdaBootstrap(
                _settings.BootstrapHttpClient,
                wrappedHandler,
                _settings.BootstrapOptions,
                null
            );

        return bootstrap.RunAsync(stoppingToken);
    }

    private void RunShutdownHandlers(IServiceScopeFactory scopeFactory)
    {
        using var scope = scopeFactory.CreateScope();
        var services = scope.ServiceProvider;
    }

    private Func<Stream, ILambdaContext, Task<Stream>> GetHandler(
        LambdaInvocationDelegate handler,
        CancellationToken stoppingToken
    ) =>
        async Task<Stream> (inputStream, lambdaContext) =>
        {
            // create a base cancellation token source using the provided token source factory
            using var cancellationTokenSource =
                _cancellationTokenSourceFactory.NewCancellationTokenSource(lambdaContext);

            // combine a base cancellation token source with the stoppingToken. This will allow for
            // cancellation when either the maximum lifetime of the lambda has been reached or the
            // lambda runtime has started to shut down.
            using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                stoppingToken,
                cancellationTokenSource.Token
            );

            await using var lambdaHostContext = new LambdaHostContext(
                lambdaContext,
                _scopeFactory,
                linkedTokenSource.Token
            );

            if (_delegateHolder.Deserializer is not null)
                await _delegateHolder.Deserializer(
                    lambdaHostContext,
                    _settings.LambdaSerializer,
                    inputStream
                );

            await handler(lambdaHostContext);

            if (_delegateHolder.Serializer is not null)
                return await _delegateHolder.Serializer(
                    lambdaHostContext,
                    _settings.LambdaSerializer
                );

            return new MemoryStream(0);
        };

    private static LambdaInvocationDelegate BuildMiddlewarePipeline(
        List<Func<LambdaInvocationDelegate, LambdaInvocationDelegate>> middlewares,
        LambdaInvocationDelegate handler
    ) =>
        middlewares
            .Reverse<Func<LambdaInvocationDelegate, LambdaInvocationDelegate>>()
            .Aggregate(handler, (next, middleware) => middleware(next));
}
