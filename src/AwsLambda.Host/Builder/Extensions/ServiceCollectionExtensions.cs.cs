using System.Text.Json.Serialization;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using AwsLambda.Host.Core.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AwsLambda.Host;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddLambdaHostCoreServices()
        {
            ArgumentNullException.ThrowIfNull(services);

            // register feature collection
            services.AddSingleton<IFeatureCollection, FeatureCollection>();

            // Register Lambda execution components
            services.AddSingleton<ILambdaHandlerFactory, LambdaHandlerComposer>();
            services.AddSingleton<ILambdaBootstrapOrchestrator, LambdaBootstrapAdapter>();
            services.AddSingleton<ILambdaLifecycleOrchestrator, LambdaLifecycleOrchestrator>();

            // Register LambdaHostedService as IHostedService
            services.AddHostedService<LambdaHostedService>();

            return services;
        }

        public IServiceCollection TryAddLambdaHostDefaultServices()
        {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAddSingleton<ILambdaSerializer, DefaultLambdaJsonSerializer>();
            services.TryAddSingleton<
                ILambdaCancellationTokenSourceFactory,
                LambdaCancellationTokenSourceFactory
            >();

            return services;
        }
    }
}
