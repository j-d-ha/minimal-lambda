using Amazon.Lambda.Core;
using AwsLambda.Host.Middleware;

namespace AwsLambda.Host;

internal sealed class DelegateHolder
{
    internal LambdaInvocationDelegate? Handler { get; set; }

    internal List<Func<LambdaInvocationDelegate, LambdaInvocationDelegate>> Middlewares { get; } =
        [];

    internal Func<ILambdaHostContext, ILambdaSerializer, Stream, Task>? Deserializer { get; set; }

    internal Func<ILambdaHostContext, ILambdaSerializer, Task<Stream>>? Serializer { get; set; }

    internal bool IsHandlerSet => Handler != null;
}
