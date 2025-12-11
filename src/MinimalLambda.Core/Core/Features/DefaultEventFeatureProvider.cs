using Amazon.Lambda.Core;

namespace MinimalLambda.Host.Core;

/// <summary>
///     Provides a default implementation of <see cref="IEventFeature" /> for Lambda event
///     deserialization. This provider is instantiated by source-generated code to handle Lambda event
///     processing using the specified <see cref="ILambdaSerializer" />.
/// </summary>
internal class DefaultEventFeatureProvider<T>(ILambdaSerializer lambdaSerializer) : IFeatureProvider
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly Type FeatureType = typeof(IEventFeature);

    /// <inheritdoc />
    public bool TryCreate(Type type, out object? feature)
    {
        feature = type == FeatureType ? new DefaultEventFeature<T>(lambdaSerializer) : null;

        return feature is not null;
    }
}
