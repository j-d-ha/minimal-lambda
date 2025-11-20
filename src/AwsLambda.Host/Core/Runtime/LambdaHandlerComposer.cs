using Amazon.Lambda.Core;
using AwsLambda.Host.Core.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AwsLambda.Host;

/// <summary>
///     Composes the Lambda handler with middleware and handles request processing. Responsible
///     for building middleware pipelines and creating request handlers with serialization,
///     deserialization, and context management.
/// </summary>
internal sealed class LambdaHandlerComposer : ILambdaHandlerFactory
{
    private readonly ILambdaCancellationFactory _cancellationFactory;
    private readonly IFeatureCollectionFactory _featureCollectionFactory;
    private readonly IInvocationBuilderFactory _invocationBuilderFactory;
    private readonly LambdaHostedServiceOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;

    public LambdaHandlerComposer(
        ILambdaCancellationFactory cancellationFactory,
        IInvocationBuilderFactory invocationBuilderFactory,
        IServiceScopeFactory serviceScopeFactory,
        ILambdaSerializer lambdaSerializer,
        IOptions<LambdaHostedServiceOptions> options,
        IFeatureCollectionFactory featureCollectionFactory
    )
    {
        ArgumentNullException.ThrowIfNull(cancellationFactory);
        ArgumentNullException.ThrowIfNull(invocationBuilderFactory);
        ArgumentNullException.ThrowIfNull(serviceScopeFactory);
        ArgumentNullException.ThrowIfNull(lambdaSerializer);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(featureCollectionFactory);

        _cancellationFactory = cancellationFactory;
        _invocationBuilderFactory = invocationBuilderFactory;
        _scopeFactory = serviceScopeFactory;
        _options = options.Value;
        _featureCollectionFactory = featureCollectionFactory;
    }

    /// <summary>
    ///     Creates a fully composed Lambda handler that includes middleware pipeline composition and
    ///     request processing. This composes BuildMiddlewarePipeline and CreateRequestHandler into a
    ///     single operation. The handler and middleware are obtained from the injected DelegateHolder.
    /// </summary>
    public Func<Stream, ILambdaContext, Task<Stream>> CreateHandler(CancellationToken stoppingToken)
    {
        var builder = _invocationBuilderFactory.CreateBuilder();

        _options.ConfigureHandlerBuilder?.Invoke(builder);

        var handler = builder.Build();

        return CreateRequestHandler;

        async Task<Stream> CreateRequestHandler(Stream inputStream, ILambdaContext lambdaContext)
        {
            // Create a base cancellation token source using the provided token source factory.
            // This allows cancellation when the maximum lifetime of the lambda has been reached.
            using var cancellationTokenSource = _cancellationFactory.NewCancellationTokenSource(
                lambdaContext
            );

            // Combine the base cancellation token source with the stoppingToken.
            // This allows for cancellation when either:
            // - The maximum lifetime of the lambda has been reached, OR
            // - The lambda runtime has started to shut down
            using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                stoppingToken,
                cancellationTokenSource.Token
            );

            var featureCollection = _featureCollectionFactory.Create();
            var rawData = new RawInvocationData
            {
                Event = inputStream,
                Response = new MemoryStream(),
            };

            // Create a new lambda host context. This will also create a new service scope
            // the first time that the service container is accessed.
            await using var lambdaHostContext = new DefaultLambdaHostContext(
                lambdaContext,
                _scopeFactory,
                builder.Properties,
                featureCollection,
                rawData,
                linkedTokenSource.Token
            );

            // Invoke the handler wrapped in the middleware pipeline.
            await handler.Invoke(lambdaHostContext);

            if (lambdaHostContext.Features.TryGet<IResponseFeature>(out var responseFeature))
                responseFeature.SerializeToStream(lambdaHostContext);

            // If no serializer is provided, return an empty stream.
            return lambdaHostContext.RawInvocationData.Response;
        }
    }
}
