using Microsoft.Extensions.DependencyInjection;

namespace MinimalLambda.Builder;

internal class DefaultLambdaOnShutdownBuilderFactory(
    IServiceProvider serviceProvider,
    IServiceScopeFactory scopeFactory,
    ILambdaLifecycleContextFactory contextFactory) : ILambdaOnShutdownBuilderFactory
{
    public ILambdaOnShutdownBuilder CreateBuilder() =>
        new LambdaOnShutdownBuilder(serviceProvider, scopeFactory, contextFactory);
}
