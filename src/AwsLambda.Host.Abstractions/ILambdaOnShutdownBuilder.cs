namespace AwsLambda.Host;

public interface ILambdaOnShutdownBuilder
{
    IServiceProvider Services { get; }

    List<LambdaShutdownDelegate> ShutdownHandlers { get; }
}
