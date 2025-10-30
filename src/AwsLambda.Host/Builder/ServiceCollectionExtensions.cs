using Microsoft.Extensions.DependencyInjection;

namespace AwsLambda.Host;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureLambdaHost(
        this IServiceCollection serviceCollection,
        Action<LambdaHostOptions> configure
    ) => serviceCollection.PostConfigure(configure);
}
