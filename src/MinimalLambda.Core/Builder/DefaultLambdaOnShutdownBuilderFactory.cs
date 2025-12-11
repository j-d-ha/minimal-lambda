using Microsoft.Extensions.DependencyInjection;

namespace MinimalLambda.Host.Builder;

internal class DefaultLambdaOnShutdownBuilderFactory(
    IServiceProvider serviceProvider,
    IServiceScopeFactory scopeFactory
) : ILambdaOnShutdownBuilderFactory
{
    public ILambdaOnShutdownBuilder CreateBuilder() =>
        new LambdaOnShutdownBuilder(serviceProvider, scopeFactory);
}
