using Amazon.Lambda.Core;

namespace MinimalLambda;

/// <summary>
///     Provides a default implementation of <see cref="IResponseFeature" /> for Lambda response
///     serialization. This provider is instantiated by source-generated code to handle Lambda response
///     processing using the specified <see cref="ILambdaSerializer" />.
/// </summary>
internal class DefaultResponseFeatureProvider<T>(ILambdaSerializer lambdaSerializer)
    : IFeatureProvider
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly Type FeatureType = typeof(IResponseFeature);

    /// <inheritdoc />
    public bool TryCreate(Type type, out object? feature)
    {
        feature = type == FeatureType ? new DefaultResponseFeature<T>(lambdaSerializer) : null;

        return feature is not null;
    }
}
