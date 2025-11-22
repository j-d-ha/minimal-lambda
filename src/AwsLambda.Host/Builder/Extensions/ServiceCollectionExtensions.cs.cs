using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using AwsLambda.Host;
using AwsLambda.Host.Core;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

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
            services.AddSingleton<ILambdaOnInitBuilderFactory, DefaultLambdaOnInitBuilderFactory>();
            services.AddSingleton<
                ILambdaOnShutdownBuilderFactory,
                DefaultLambdaOnShutdownBuilderFactory
            >();
            services.AddSingleton<IFeatureCollectionFactory, DefaultFeatureCollectionFactory>();

            // Register internal Lambda execution components
            services.AddSingleton<ILambdaHandlerFactory, LambdaHandlerComposer>();
            services.AddSingleton<ILambdaBootstrapOrchestrator, LambdaBootstrapAdapter>();

            // Register LambdaHostedService as IHostedService
            services.AddHostedService<LambdaHostedService>();

            // Register options related services
            services.AddSingleton<
                IPostConfigureOptions<HostOptions>,
                HostOptionsPostConfiguration
            >();

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
