using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AwsLambda.Host.Builder.Extensions;

public static class LambdaHttpClientServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddLambdaBootstrapHttpClient<T>(T client)
            where T : HttpClient
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddKeyedSingleton<HttpClient>(typeof(ILambdaBootstrapOrchestrator), client);

            return services;
        }

        public IServiceCollection AddLambdaBootstrapHttpClient(
            Func<IServiceProvider, object?, HttpClient> factory
        )
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddKeyedSingleton<HttpClient>(typeof(ILambdaBootstrapOrchestrator), factory);

            return services;
        }

        public void TryAddLambdaBootstrapHttpClient<T>(T client)
            where T : HttpClient
        {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAddKeyedSingleton<HttpClient>(typeof(ILambdaBootstrapOrchestrator), client);
        }

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
