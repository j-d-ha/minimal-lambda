namespace MinimalLambda.Builder;

internal class DefaultLambdaInvocationBuilderFactory(IServiceProvider serviceProvider)
    : ILambdaInvocationBuilderFactory
{
    public ILambdaInvocationBuilder CreateBuilder() => new LambdaInvocationBuilder(serviceProvider);
}
