using AwsLambda.Host.Middleware;

namespace AwsLambda.Host;

internal sealed class DelegateHolder
{
    internal LambdaInvocationDelegate? Handler { get; set; }

    internal List<Func<LambdaInvocationDelegate, LambdaInvocationDelegate>> Middlewares { get; } =
        [];

    internal Action<ILambdaHostContext, Stream>? Deserializer { get; set; }

    internal Func<ILambdaHostContext, Stream>? Serializer { get; set; }

    internal bool IsHandlerSet => Handler != null;
}
