using Microsoft.Extensions.DependencyInjection;

namespace AwsLambda.Host;

internal class DefaultOnShutdownBuilderFactory(
    IServiceProvider serviceProvider,
    IServiceScopeFactory scopeFactory
) : IOnShutdownBuilderFactory
{
    public ILambdaOnShutdownBuilder CreateBuilder() =>
        new LambdaOnShutdownBuilder(serviceProvider, scopeFactory);
}
