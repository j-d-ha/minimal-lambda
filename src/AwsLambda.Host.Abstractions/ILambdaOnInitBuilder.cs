namespace AwsLambda.Host;

public interface ILambdaOnInitBuilder
{
    IServiceProvider Services { get; }

    List<LambdaInitDelegate> InitHandlers { get; }
}
