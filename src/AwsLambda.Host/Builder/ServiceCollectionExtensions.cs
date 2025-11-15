using AwsLambda.Host.Options;
using Microsoft.Extensions.DependencyInjection;

namespace AwsLambda.Host;

/// <summary>Provides extension methods for configuring Lambda host services.</summary>
public static class ServiceCollectionExtensions
{
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
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to configure.</param>
    /// <param name="options">An action that configures the <see cref="LambdaHostOptions" /> instance.</param>
    /// <returns>The <see cref="IServiceCollection" /> for method chaining.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="serviceCollection" /> or
    ///     <paramref name="options" /> is <c>null</c>.
    /// </exception>
    /// <seealso cref="LambdaHostOptions" />
    public static IServiceCollection ConfigureLambdaHostOptions(
        this IServiceCollection serviceCollection,
        Action<LambdaHostOptions> options
    )
    {
        ArgumentNullException.ThrowIfNull(serviceCollection);
        ArgumentNullException.ThrowIfNull(options);

        return serviceCollection.PostConfigure(options);
    }

    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection ConfigureEnvelopeOptions(Action<EnvelopeOptions> options)
        {
            ArgumentNullException.ThrowIfNull(serviceCollection);
            ArgumentNullException.ThrowIfNull(options);

            return serviceCollection.PostConfigure(options);
        }
    }
}
