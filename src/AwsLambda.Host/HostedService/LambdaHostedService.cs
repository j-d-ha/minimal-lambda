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

    private readonly List<Exception> _exceptions = [];
    private readonly IHostApplicationLifetime _lifetime;
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
        _lifetime = lifetime;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Build the middleware pipeline and wrap the handler.
        var handler = BuildMiddlewarePipeline(
            _delegateHolder.Middlewares,
            _delegateHolder.Handler!
        );

        // wrap the handler with HandlerWrapper. We do this is because there is no public
        // constructor for LambdaBootstrap that accepts settings, initializer, and http client.
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

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        // await the background service stop and capture any exceptions that occur.
        try
        {
            await base.StopAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _exceptions.Add(ex);
        }

        // if any exceptions were captured, rethrow them.
        if (_exceptions.Count > 0)
            throw new AggregateException(_exceptions);
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

            // create a new lambda host context. This will also create a new service scope the first
            // time that the service container is accessed.
            await using var lambdaHostContext = new LambdaHostContext(
                lambdaContext,
                _scopeFactory,
                linkedTokenSource.Token
            );

            // deserialize the request if a deserializer is provided.
            if (_delegateHolder.Deserializer is not null)
                await _delegateHolder.Deserializer(
                    lambdaHostContext,
                    _settings.LambdaSerializer,
                    inputStream
                );

            // invoke the handler wrapped in the middleware pipeline.
            await handler(lambdaHostContext);

            // serialize the response if a serializer is provided.
            if (_delegateHolder.Serializer is not null)
                return await _delegateHolder.Serializer(
                    lambdaHostContext,
                    _settings.LambdaSerializer
                );

            // if no serializer is provided, return an empty stream.
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
