namespace AwsLambda.Host.Core;

/// <summary>Creates feature providers for Lambda response serialization.</summary>
/// <remarks>
///     <para>
///         <see cref="IResponseFeatureProviderFactory" /> creates <see cref="IFeatureProvider" />
///         instances for specific response types during Lambda invocations. The factory enables
///         lazy registration of response feature providers. This factory is registered automatically
///         at startup.
///     </para>
/// </remarks>
public interface IResponseFeatureProviderFactory
{
    /// <summary>Creates a feature provider for the specified response type.</summary>
    /// <typeparam name="T">The type of response object to create a provider for.</typeparam>
    /// <returns>
    ///     An <see cref="IFeatureProvider" /> that can create <see cref="IResponseFeature" />
    ///     instances for the specified type.
    /// </returns>
    IFeatureProvider Create<T>();
}
