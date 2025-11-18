namespace AwsLambda.Host;

internal class LambdaOnInitBuilder : ILambdaOnInitBuilder
{
    public IServiceProvider Services { get; }
    public List<LambdaInitDelegate> InitHandlers { get; } = [];

    public LambdaOnInitBuilder(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        Services = serviceProvider;
    }
}
