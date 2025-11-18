namespace AwsLambda.Host;

internal class LambdaHandlerBuilderFactory : ILambdaHandlerBuilderFactory
{
    private readonly IServiceProvider _serviceProvider;

    public LambdaHandlerBuilderFactory(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _serviceProvider = serviceProvider;
    }

    public ILambdaHandlerBuilder CreateBuilder() => new LambdaHandlerBuilder(_serviceProvider);
}
