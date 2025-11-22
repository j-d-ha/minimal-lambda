using Microsoft.Extensions.DependencyInjection;

namespace AwsLambda.Host.Builder;

internal class DefaultLambdaOnShutdownBuilderFactory(
    IServiceProvider serviceProvider,
    IServiceScopeFactory scopeFactory
) : ILambdaOnShutdownBuilderFactory
{
    public ILambdaOnShutdownBuilder CreateBuilder() =>
        new LambdaOnShutdownBuilder(serviceProvider, scopeFactory);
}
