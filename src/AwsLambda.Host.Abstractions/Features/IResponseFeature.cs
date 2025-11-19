namespace AwsLambda.Host;

public interface IResponseFeature
{
    object? GetResponse();

    void SetResponse(object? response);

    void SerializeToStream(ILambdaHostContext context);
}
