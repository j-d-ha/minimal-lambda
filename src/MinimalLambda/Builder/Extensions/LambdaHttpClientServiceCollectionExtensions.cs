using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MinimalLambda.Builder.Extensions;

/// <summary>
/// Extension methods for configuring the Lambda bootstrap HTTP client in an
/// <see cref="IServiceCollection" />.
/// </summary>
public static class LambdaHttpClientServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>Adds a custom HTTP client instance for the Lambda bootstrap runtime API.</summary>
        /// <typeparam name="T">The type of <see cref="HttpClient" /> to register.</typeparam>
        /// <param name="client">The pre-configured <see cref="HttpClient" /> instance.</param>
        /// <returns>The <see cref="IServiceCollection" /> for chaining.</returns>
        /// <remarks>
        ///     Registers a keyed singleton <see cref="HttpClient" /> that will be used by the Lambda
        ///     bootstrap to communicate with the Lambda runtime API. This overload is useful for testing
        ///     scenarios where you want to inject a mock or fake HTTP client.
        /// </remarks>
        public IServiceCollection AddLambdaBootstrapHttpClient<T>(T client)
            where T : HttpClient
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddKeyedSingleton<HttpClient>(typeof(ILambdaBootstrapOrchestrator), client);

            return services;
        }

        /// <summary>
        /// Adds a factory for creating a custom HTTP client for the Lambda bootstrap runtime API.
        /// </summary>
        /// <param name="factory">
        /// A factory function that creates the <see cref="HttpClient" /> instance. The function
        /// receives the <see cref="IServiceProvider" /> and service key.
        /// </param>
        /// <returns>The <see cref="IServiceCollection" /> for chaining.</returns>
        /// <remarks>
        ///     <para>
        ///         Registers a keyed singleton <see cref="HttpClient" /> factory that will be used by the
        ///         Lambda bootstrap to communicate with the Lambda runtime API. This overload provides
        ///         access to the dependency injection container, allowing you to resolve other services
        ///         when constructing the HTTP client.
        ///     </para>
        /// </remarks>
        public IServiceCollection AddLambdaBootstrapHttpClient(
            Func<IServiceProvider, object?, HttpClient> factory
        )
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddKeyedSingleton<HttpClient>(typeof(ILambdaBootstrapOrchestrator), factory);

            return services;
        }

        /// <summary>
        /// Attempts to add a custom HTTP client instance for the Lambda bootstrap runtime API if one is
        /// not already registered.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="HttpClient" /> to register.</typeparam>
        /// <param name="client">The pre-configured <see cref="HttpClient" /> instance.</param>
        /// <remarks>
        ///     Registers a keyed singleton <see cref="HttpClient" /> that will be used by the Lambda
        ///     bootstrap to communicate with the Lambda runtime API, but only if a keyed HTTP client has
        ///     not already been registered. This overload is useful for testing scenarios where you want
        ///     to inject a mock or fake HTTP client without overwriting user-supplied configurations.
        /// </remarks>
        public void TryAddLambdaBootstrapHttpClient<T>(T client)
            where T : HttpClient
        {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAddKeyedSingleton<HttpClient>(typeof(ILambdaBootstrapOrchestrator), client);
        }

        /// <summary>
        /// Attempts to add a factory for creating a custom HTTP client for the Lambda bootstrap runtime
        /// API if one is not already registered.
        /// </summary>
        /// <param name="factory">
        /// A factory function that creates the <see cref="HttpClient" /> instance. The function
        /// receives the <see cref="IServiceProvider" /> and service key.
        /// </param>
        /// <remarks>
        ///     <para>
        ///         Registers a keyed singleton <see cref="HttpClient" /> factory that will be used by the
        ///         Lambda bootstrap to communicate with the Lambda runtime API, but only if a keyed HTTP
        ///         client has not already been registered.
        ///     </para>
        /// </remarks>
        public void TryAddLambdaBootstrapHttpClient(
            Func<IServiceProvider, object?, HttpClient> factory
        )
        {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAddKeyedSingleton<HttpClient>(
                typeof(ILambdaBootstrapOrchestrator),
                factory
            );
        }
    }
}
