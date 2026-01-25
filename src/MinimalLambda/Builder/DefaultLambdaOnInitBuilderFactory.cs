using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MinimalLambda.Builder;

internal class DefaultLambdaOnInitBuilderFactory(
    IServiceProvider serviceProvider,
    IServiceScopeFactory scopeFactory,
    IOptions<LambdaHostOptions> options,
    ILambdaLifecycleContextFactory contextFactory) : ILambdaOnInitBuilderFactory
{
    public ILambdaOnInitBuilder CreateBuilder() =>
        new LambdaOnInitBuilder(serviceProvider, scopeFactory, options, contextFactory);
}
