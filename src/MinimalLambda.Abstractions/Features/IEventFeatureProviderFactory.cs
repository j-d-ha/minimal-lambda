namespace MinimalLambda;

/// <summary>Creates feature providers for Lambda event deserialization.</summary>
/// <remarks>
///     <para>
///         <see cref="IEventFeatureProviderFactory" /> creates <see cref="IFeatureProvider" />
///         instances for specific event types during Lambda invocations. The factory enables
///         lazy registration of event feature providers. This factory is registered automatically
///         at startup.
///     </para>
/// </remarks>
public interface IEventFeatureProviderFactory
{
    /// <summary>Creates a feature provider for the specified event type.</summary>
    /// <typeparam name="T">The type of event object to create a provider for.</typeparam>
    /// <returns>
    ///     An <see cref="IFeatureProvider" /> that can create <see cref="IEventFeature" />
    ///     instances for the specified type.
    /// </returns>
    IFeatureProvider Create<T>();
}
