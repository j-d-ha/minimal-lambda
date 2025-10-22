using Microsoft.Extensions.DependencyInjection;

namespace Lambda.Host;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureLambdaHost(
        this IServiceCollection serviceCollection,
        Action<LambdaHostSettings> configure
    ) => serviceCollection.PostConfigure(configure);
}
