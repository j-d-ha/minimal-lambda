using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Lambda.Host.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Lambda.Host;

public class LambdaHostedService : IHostedService
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
        var handler = HandlerWrapper.GetHandlerWrapper(
            async Task<Stream> (Stream inputStream, ILambdaContext lambdaContext) =>
            {
                using var cancellationTokenSource =
                    _cancellationTokenSourceFactory.NewCancellationTokenSource(lambdaContext);

                await using var lambdaHostContext = new LambdaHostContext(
                    lambdaContext,
                    _scopeFactory,
                    cancellationTokenSource.Token,
                    inputStream,
                    _settings.LambdaSerializer
                );

                var handler = BuildMiddlewarePipeline(_delegateHolder.Handler!);

                await handler(lambdaHostContext);

                if (lambdaHostContext.ResponseStream == null)
                    return new MemoryStream(0);

                return lambdaHostContext.ResponseStream;
            }
        );

        var bootstrap = _settings.BootstrapHttpClient is null
            ? new LambdaBootstrap(handler, _settings.BootstrapOptions, null)
            : new LambdaBootstrap(
                _settings.BootstrapHttpClient,
                handler,
                _settings.BootstrapOptions,
                null
            );

        bootstrap.RunAsync(cancellationToken);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private LambdaInvocationDelegate BuildMiddlewarePipeline(LambdaInvocationDelegate handler)
    {
        var pipeline = handler;

        for (var i = _delegateHolder.Middlewares.Count - 1; i >= 0; i--)
        {
            var middleware = _delegateHolder.Middlewares[i];
            pipeline = middleware(pipeline);
        }

        return pipeline;
    }
}
