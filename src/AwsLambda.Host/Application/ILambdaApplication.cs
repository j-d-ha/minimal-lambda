using AwsLambda.Host.Middleware;

namespace AwsLambda.Host;

public interface ILambdaApplication
{
    ILambdaApplication MapHandler(
        LambdaInvocationDelegate handler,
        Action<ILambdaHostContext, Stream>? deserializer = null,
        Func<ILambdaHostContext, Stream>? serializer = null
    );

    ILambdaApplication Use(Func<LambdaInvocationDelegate, LambdaInvocationDelegate> middleware);
}
