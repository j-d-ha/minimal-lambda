using Amazon.Lambda.Core;

namespace AwsLambda.Host;

public interface ILambdaApplication
{
    IServiceProvider Services { get; }

    ILambdaApplication Map(
        LambdaInvocationDelegate handler,
        Func<ILambdaHostContext, ILambdaSerializer, Stream, Task>? deserializer,
        Func<ILambdaHostContext, ILambdaSerializer, Task<Stream>>? serializer
    );

    ILambdaApplication Use(Func<LambdaInvocationDelegate, LambdaInvocationDelegate> middleware);

    ILambdaApplication OnShutdown(LambdaShutdownDelegate handler);
}
