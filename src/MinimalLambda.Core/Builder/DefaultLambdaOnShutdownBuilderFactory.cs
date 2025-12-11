using Microsoft.Extensions.DependencyInjection;

namespace MinimalLambda.Builder;

internal class DefaultLambdaOnShutdownBuilderFactory(
    IServiceProvider serviceProvider,
    IServiceScopeFactory scopeFactory
) : ILambdaOnShutdownBuilderFactory
{
    public ILambdaOnShutdownBuilder CreateBuilder() =>
        new LambdaOnShutdownBuilder(serviceProvider, scopeFactory);
}
