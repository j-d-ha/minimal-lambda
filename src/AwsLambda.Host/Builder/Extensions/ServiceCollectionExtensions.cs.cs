using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using AwsLambda.Host.Core.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AwsLambda.Host;

/// <summary>Extension methods for registering Lambda Host services.</summary>
public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>Registers core Lambda Host services into the dependency injection container.</summary>
        /// <returns>The service collection for chaining.</returns>
        public IServiceCollection AddLambdaHostCoreServices()
        {
            ArgumentNullException.ThrowIfNull(services);

            // register core factories
            services.AddSingleton<
                ILambdaInvocationBuilderFactory,
                DefaultLambdaInvocationBuilderFactory
            >();
            services.AddSingleton<IOnInitBuilderFactory, DefaultOnInitBuilderFactory>();
            services.AddSingleton<IOnShutdownBuilderFactory, DefaultOnShutdownBuilderFactory>();
            services.AddSingleton<IFeatureCollectionFactory, DefaultFeatureCollectionFactory>();

            // Register internal Lambda execution components
            services.AddSingleton<ILambdaHandlerFactory, LambdaHandlerComposer>();
            services.AddSingleton<ILambdaBootstrapOrchestrator, LambdaBootstrapAdapter>();

            // Register LambdaHostedService as IHostedService
            services.AddHostedService<LambdaHostedService>();

            return services;
        }

        /// <summary>
        ///     Conditionally registers default Lambda Host services if they haven't been registered
        ///     already.
        /// </summary>
        /// <returns>The service collection for chaining.</returns>
        public IServiceCollection TryAddLambdaHostDefaultServices()
        {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAddSingleton<ILambdaSerializer, DefaultLambdaJsonSerializer>();
            services.TryAddSingleton<
                ILambdaCancellationFactory,
                DefaultLambdaCancellationFactory
            >();

            return services;
        }
    }
}
