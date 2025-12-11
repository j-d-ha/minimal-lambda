using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MinimalLambda.Host.Builder;

internal class DefaultLambdaOnInitBuilderFactory(
    IServiceProvider serviceProvider,
    IServiceScopeFactory scopeFactory,
    IOptions<LambdaHostOptions> options
) : ILambdaOnInitBuilderFactory
{
    public ILambdaOnInitBuilder CreateBuilder() =>
        new LambdaOnInitBuilder(serviceProvider, scopeFactory, options);
}
