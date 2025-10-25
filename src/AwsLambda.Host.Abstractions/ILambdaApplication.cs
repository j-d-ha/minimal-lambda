using Amazon.Lambda.Core;
using AwsLambda.Host.Middleware;

namespace AwsLambda.Host;

public interface ILambdaApplication
{
    ILambdaApplication Map(
        LambdaInvocationDelegate handler,
        Func<ILambdaHostContext, ILambdaSerializer, Stream, Task>? deserializer,
        Func<ILambdaHostContext, ILambdaSerializer, Task<Stream>>? serializer
    );

    ILambdaApplication Use(Func<LambdaInvocationDelegate, LambdaInvocationDelegate> middleware);
}
