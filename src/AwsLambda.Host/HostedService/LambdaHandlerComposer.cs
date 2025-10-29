using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AwsLambda.Host;

/// <summary>
/// Composes the Lambda handler with middleware and handles request processing.
/// Responsible for building middleware pipelines and creating request handlers
/// with serialization, deserialization, and context management.
/// </summary>
internal sealed class LambdaHandlerComposer : ILambdaHandlerFactory
{
    private readonly ILambdaCancellationTokenSourceFactory _cancellationTokenSourceFactory;
    private readonly DelegateHolder _delegateHolder;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly LambdaHostSettings _settings;

    public LambdaHandlerComposer(
        IOptions<LambdaHostSettings> lambdaHostSettings,
        DelegateHolder delegateHolder,
        ILambdaCancellationTokenSourceFactory cancellationTokenSourceFactory,
        IServiceScopeFactory serviceScopeFactory
    )
    {
        ArgumentNullException.ThrowIfNull(lambdaHostSettings);
        ArgumentNullException.ThrowIfNull(delegateHolder);
        ArgumentNullException.ThrowIfNull(cancellationTokenSourceFactory);
        ArgumentNullException.ThrowIfNull(serviceScopeFactory);

        if (!delegateHolder.IsHandlerSet)
            throw new InvalidOperationException("Lambda Handler is not set");

        _settings = lambdaHostSettings.Value;
        _delegateHolder = delegateHolder;
        _cancellationTokenSourceFactory = cancellationTokenSourceFactory;
        _scopeFactory = serviceScopeFactory;
    }

    /// <summary>
    /// Creates a fully composed Lambda handler that includes middleware pipeline composition
    /// and request processing. This composes BuildMiddlewarePipeline and CreateRequestHandler
    /// into a single operation. The handler and middleware are obtained from the injected DelegateHolder.
    /// </summary>
    public Func<Stream, ILambdaContext, Task<Stream>> CreateHandler(CancellationToken stoppingToken)
    {
        var composedHandler = BuildMiddlewarePipeline(
            _delegateHolder.Middlewares,
            _delegateHolder.Handler!
        );
        return CreateRequestHandler(composedHandler, stoppingToken);
    }

    /// <summary>
    /// Builds a middleware pipeline by reversing the middleware list and aggregating
    /// them around the core handler.
    /// </summary>
    /// <remarks>
    /// Middleware is applied in reverse order so that the first middleware in the list
    /// is the outermost (first to execute), ensuring intuitive ordering.
    /// </remarks>
    private LambdaInvocationDelegate BuildMiddlewarePipeline(
        List<Func<LambdaInvocationDelegate, LambdaInvocationDelegate>> middlewares,
        LambdaInvocationDelegate handler
    ) =>
        middlewares
            .Reverse<Func<LambdaInvocationDelegate, LambdaInvocationDelegate>>()
            .Aggregate(handler, (next, middleware) => middleware(next));

    /// <summary>
    /// Creates a handler function that processes a single Lambda invocation.
    /// Handles cancellation coordination, context creation, and serialization.
    /// </summary>
    private Func<Stream, ILambdaContext, Task<Stream>> CreateRequestHandler(
        LambdaInvocationDelegate handler,
        CancellationToken stoppingToken
    ) =>
        async Task<Stream> (inputStream, lambdaContext) =>
        {
            // Create a base cancellation token source using the provided token source factory.
            // This allows cancellation when the maximum lifetime of the lambda has been reached.
            using var cancellationTokenSource =
                _cancellationTokenSourceFactory.NewCancellationTokenSource(lambdaContext);

            // Combine the base cancellation token source with the stoppingToken.
            // This allows for cancellation when either:
            // - The maximum lifetime of the lambda has been reached, OR
            // - The lambda runtime has started to shut down
            using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                stoppingToken,
                cancellationTokenSource.Token
            );

            // Create a new lambda host context. This will also create a new service scope
            // the first time that the service container is accessed.
            await using var lambdaHostContext = new LambdaHostContext(
                lambdaContext,
                _scopeFactory,
                linkedTokenSource.Token
            );

            // Deserialize the request if a deserializer is provided.
            if (_delegateHolder.Deserializer is not null)
                await _delegateHolder.Deserializer(
                    lambdaHostContext,
                    _settings.LambdaSerializer,
                    inputStream
                );

            // Invoke the handler wrapped in the middleware pipeline.
            await handler(lambdaHostContext);

            // Serialize the response if a serializer is provided.
            if (_delegateHolder.Serializer is not null)
                return await _delegateHolder.Serializer(
                    lambdaHostContext,
                    _settings.LambdaSerializer
                );

            // If no serializer is provided, return an empty stream.
            return new MemoryStream(0);
        };
}
