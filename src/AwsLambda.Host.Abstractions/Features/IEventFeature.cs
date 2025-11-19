namespace AwsLambda.Host;

public interface IEventFeature
{
    object? GetEvent(ILambdaHostContext context);
}
