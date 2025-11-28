namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Provides extension methods for configuring Lambda host services.</summary>
public static class ConfigurationServiceCollectionExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        /// <summary>Configures envelope options in the service collection.</summary>
        /// <remarks>
        ///     <para>
        ///         Allows you to configure <see cref="EnvelopeOptions" /> by providing an action that
        ///         modifies the options instance. The configuration is applied after any initial configuration
        ///         from configuration files or other sources.
        ///     </para>
        /// </remarks>
        /// <param name="options">An action that configures the <see cref="EnvelopeOptions" /> instance.</param>
        /// <returns>The <see cref="IServiceCollection" /> for method chaining.</returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <see cref="IServiceCollection" /> or
        ///     <paramref name="options" /> is <c>null</c>.
        /// </exception>
        /// <seealso cref="EnvelopeOptions" />
        public IServiceCollection ConfigureEnvelopeOptions(Action<EnvelopeOptions> options)
        {
            ArgumentNullException.ThrowIfNull(serviceCollection);
            ArgumentNullException.ThrowIfNull(options);

            return serviceCollection.Configure(options);
        }

        /// <summary>Configures the Lambda Host options in the service collection.</summary>
        /// <remarks>
        ///     <para>
        ///         This extension method allows you to configure <see cref="LambdaHostOptions" /> by providing
        ///         an action that modifies the options instance. The configuration is applied using
        ///         <see
        ///             cref="Microsoft.Extensions.DependencyInjection.OptionsServiceCollectionExtensions.PostConfigure{TOptions}(IServiceCollection, Action{TOptions})" />
        ///         which means it runs after any initial configuration from configuration files or other
        ///         sources.
        ///     </para>
        /// </remarks>
        /// <param name="options">An action that configures the <see cref="LambdaHostOptions" /> instance.</param>
        /// <returns>The <see cref="IServiceCollection" /> for method chaining.</returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <see cref="IServiceCollection" /> or
        ///     <paramref name="options" /> is <c>null</c>.
        /// </exception>
        /// <seealso cref="LambdaHostOptions" />
        public IServiceCollection ConfigureLambdaHostOptions(Action<LambdaHostOptions> options)
        {
            ArgumentNullException.ThrowIfNull(serviceCollection);
            ArgumentNullException.ThrowIfNull(options);

            return serviceCollection.Configure(options);
        }
    }
}
