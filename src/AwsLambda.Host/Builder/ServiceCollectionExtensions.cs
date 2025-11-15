using System.Text.Json.Serialization;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
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

            return serviceCollection.PostConfigure(options);
        }

        /// <summary>Adds a Lambda JSON serializer configured with a source-generated serialization context.</summary>
        /// <remarks>
        ///     <para>
        ///         Registers a <see cref="SourceGeneratorLambdaJsonSerializer{TContext}" /> as the
        ///         <see cref="ILambdaSerializer" /> in the service collection. This method is designed to work
        ///         with source-generated JSON serialization contexts (derived from
        ///         <see cref="System.Text.Json.Serialization.JsonSerializerContext" />), which provide
        ///         compile-time serialization metadata and improved performance.
        ///     </para>
        /// </remarks>
        /// <typeparam name="TContext">
        ///     A <see cref="JsonSerializerContext" /> type that contains the
        ///     source-generated serialization metadata for your Lambda event and response types.
        /// </typeparam>
        /// <returns>The <see cref="IServiceCollection" /> for method chaining.</returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <see cref="IServiceCollection" /> is
        ///     <c>null</c>.
        /// </exception>
        /// <seealso cref="SourceGeneratorLambdaJsonSerializer{TContext}" />
        /// <seealso cref="JsonSerializerContext" />
        public IServiceCollection AddLambdaSerializerWithContext<TContext>()
            where TContext : JsonSerializerContext
        {
            ArgumentNullException.ThrowIfNull(serviceCollection);

            serviceCollection.AddSingleton<ILambdaSerializer>(
                _ => new SourceGeneratorLambdaJsonSerializer<TContext>()
            );

            return serviceCollection;
        }
    }
}
