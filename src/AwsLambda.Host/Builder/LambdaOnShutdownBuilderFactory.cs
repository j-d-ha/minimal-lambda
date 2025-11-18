namespace AwsLambda.Host;

public class LambdaOnShutdownBuilderFactory : ILambdaOnShutdownBuilderFactory
{
    private readonly IServiceProvider _serviceProvider;

    public LambdaOnShutdownBuilderFactory(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _serviceProvider = serviceProvider;
    }

    public ILambdaOnShutdownBuilder CreateBuilder() =>
        new LambdaOnShutdownBuilder(_serviceProvider);
}
