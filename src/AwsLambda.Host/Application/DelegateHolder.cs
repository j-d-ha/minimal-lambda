using Amazon.Lambda.Core;

namespace AwsLambda.Host;

internal sealed class DelegateHolder
{
    internal LambdaInvocationDelegate? Handler { get; set; }

    internal List<Func<LambdaInvocationDelegate, LambdaInvocationDelegate>> Middlewares { get; } =
        [];

    internal Func<ILambdaHostContext, ILambdaSerializer, Stream, Task>? Deserializer { get; set; }

    internal Func<ILambdaHostContext, ILambdaSerializer, Task<Stream>>? Serializer { get; set; }

    internal List<LambdaLifecycleDelegate> StartupHandlers { get; } = [];

    internal List<LambdaLifecycleDelegate> ShutdownHandlers { get; } = [];

    internal bool IsHandlerSet => Handler != null;
}
