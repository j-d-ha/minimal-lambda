using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;

namespace AwsLambda.Host.Core;

internal class LambdaHostContextFactory : ILambdaHostContextFactory
{
    private readonly ILambdaHostContextAccessor? _contextAccessor;
    private readonly IFeatureCollectionFactory _featureCollectionFactory;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public LambdaHostContextFactory(
        IServiceScopeFactory serviceScopeFactory,
        IFeatureCollectionFactory featureCollectionFactory,
        ILambdaHostContextAccessor? contextAccessor = null
    )
    {
        ArgumentNullException.ThrowIfNull(serviceScopeFactory);
        ArgumentNullException.ThrowIfNull(featureCollectionFactory);

        _serviceScopeFactory = serviceScopeFactory;
        _contextAccessor = contextAccessor;
        _featureCollectionFactory = featureCollectionFactory;
    }

    public ILambdaHostContext Create(
        ILambdaContext lambdaContext,
        IDictionary<string, object?> properties,
        RawInvocationData rawData,
        CancellationToken cancellationToken
    )
    {
        var context = new DefaultLambdaHostContext(
            lambdaContext,
            _serviceScopeFactory,
            properties,
            _featureCollectionFactory.Create(),
            rawData,
            cancellationToken
        );

        _contextAccessor?.LambdaHostContext = context;

        return context;
    }
}
