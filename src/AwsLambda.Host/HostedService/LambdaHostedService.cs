using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using AwsLambda.Host.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace AwsLambda.Host;

internal class LambdaHostedService : IHostedService
{
    private readonly ILambdaCancellationTokenSourceFactory _cancellationTokenSourceFactory;
    private readonly DelegateHolder _delegateHolder;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly LambdaHostSettings _settings;

    public LambdaHostedService(
        IOptions<LambdaHostSettings> lambdaHostSettings,
        DelegateHolder delegateHolder,
        ILambdaCancellationTokenSourceFactory lambdaCancellationTokenSourceFactory,
        IServiceScopeFactory serviceScopeFactory
    )
    {
        _settings =
            lambdaHostSettings.Value ?? throw new ArgumentNullException(nameof(lambdaHostSettings));
        _delegateHolder = delegateHolder ?? throw new ArgumentNullException(nameof(delegateHolder));
        _cancellationTokenSourceFactory =
            lambdaCancellationTokenSourceFactory
            ?? throw new ArgumentNullException(nameof(lambdaCancellationTokenSourceFactory));
        _scopeFactory =
            serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));

        if (!_delegateHolder.IsHandlerSet)
            throw new InvalidOperationException("Handler is not set");
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Build the middleware pipeline and wrap the handler.
        var handler = BuildMiddlewarePipeline(
            _delegateHolder.Middlewares,
            _delegateHolder.Handler!
        );

        var wrappedHandler = HandlerWrapper.GetHandlerWrapper(
            async Task<Stream> (Stream inputStream, ILambdaContext lambdaContext) =>
            {
                using var cancellationTokenSource =
                    _cancellationTokenSourceFactory.NewCancellationTokenSource(lambdaContext);

                await using var lambdaHostContext = new LambdaHostContext(
                    lambdaContext,
                    _scopeFactory,
                    cancellationTokenSource.Token,
                    _settings.LambdaSerializer
                );

                _delegateHolder.Deserializer?.Invoke(lambdaHostContext, inputStream);

                await handler(lambdaHostContext);

                return _delegateHolder.Serializer?.Invoke(lambdaHostContext) ?? new MemoryStream(0);
            }
        );

        var bootstrap = _settings.BootstrapHttpClient is null
            ? new LambdaBootstrap(wrappedHandler, _settings.BootstrapOptions, null)
            : new LambdaBootstrap(
                _settings.BootstrapHttpClient,
                wrappedHandler,
                _settings.BootstrapOptions,
                null
            );

        bootstrap.RunAsync(cancellationToken);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static LambdaInvocationDelegate BuildMiddlewarePipeline(
        List<Func<LambdaInvocationDelegate, LambdaInvocationDelegate>> middlewares,
        LambdaInvocationDelegate handler
    ) =>
        middlewares
            .Reverse<Func<LambdaInvocationDelegate, LambdaInvocationDelegate>>()
            .Aggregate(handler, (next, middleware) => middleware(next));
}
