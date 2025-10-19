using Lambda.Host.Middleware;

namespace Lambda.Host;

public sealed class DelegateHolder
{
    public LambdaInvocationDelegate? Handler { get; set; }

    public List<Func<LambdaInvocationDelegate, LambdaInvocationDelegate>> Middlewares { get; } = [];

    public bool IsHandlerSet => Handler != null;
}
