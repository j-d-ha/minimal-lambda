#region

using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;

#endregion

namespace AwsLambda.Host.Core;

internal class LambdaHostContextFactory : ILambdaHostContextFactory
{
    private readonly ILambdaHostContextAccessor? _contextAccessor;
    private readonly IFeatureCollectionFactory _featureCollectionFactory;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private IFeatureProvider[]? _featureProviders;

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
        CancellationToken cancellationToken
    )
    {
        _featureProviders ??= CreateFeatureProviders(properties);

        var context = new DefaultLambdaHostContext(
            lambdaContext,
            _serviceScopeFactory,
            properties,
            _featureCollectionFactory.Create(_featureProviders),
            cancellationToken
        );

        _contextAccessor?.LambdaHostContext = context;

        return context;
    }

    private static IFeatureProvider[] CreateFeatureProviders(
        IDictionary<string, object?> properties
    )
    {
        var list = new List<IFeatureProvider>(2);

        if (
            properties.TryGetValue(
                LambdaInvocationBuilder.EventFeatureProviderKey,
                out var eventObj
            ) && eventObj is IFeatureProvider eventFeatureProvider
        )
            list.Add(eventFeatureProvider);

        if (
            properties.TryGetValue(
                LambdaInvocationBuilder.ResponseFeatureProviderKey,
                out var responseObj
            ) && responseObj is IFeatureProvider responseFeatureProvider
        )
            list.Add(responseFeatureProvider);

        return list.ToArray();
    }
}
