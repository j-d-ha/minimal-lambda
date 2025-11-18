namespace AwsLambda.Host;

internal class LambdaOnInitBuilderFactory : ILambdaOnInitBuilderFactory
{
    private readonly IServiceProvider _serviceProvider;

    public LambdaOnInitBuilderFactory(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _serviceProvider = serviceProvider;
    }

    public ILambdaOnInitBuilder CreateBuilder() => new LambdaOnInitBuilder(_serviceProvider);
}
