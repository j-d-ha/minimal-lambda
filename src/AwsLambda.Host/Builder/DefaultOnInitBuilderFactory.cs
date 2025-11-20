using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AwsLambda.Host;

internal class DefaultOnInitBuilderFactory(
    IServiceProvider serviceProvider,
    IServiceScopeFactory scopeFactory,
    IOptions<LambdaHostOptions> options
) : IOnInitBuilderFactory
{
    public ILambdaOnInitBuilder CreateBuilder() =>
        new LambdaOnInitBuilder(serviceProvider, scopeFactory, options);
}
