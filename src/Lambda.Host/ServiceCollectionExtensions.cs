using Microsoft.Extensions.DependencyInjection;

namespace Lambda.Host;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureLambdaHost(
        this IServiceCollection serviceCollection,
        Action<LambdaHostSettings> configure
    ) => serviceCollection.PostConfigure(configure);

    public static IServiceCollection AddLambdaHostedService<T>(
        this IServiceCollection serviceCollection
    )
        where T : LambdaHostedService => serviceCollection.AddSingleton<LambdaHostedService, T>();
}
