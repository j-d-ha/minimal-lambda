namespace AwsLambda.Host.Testing;

public class InitResponse
{
    public ErrorResponse? Error { get; internal set; }
    public bool InitSuccess { get; internal set; }
}
