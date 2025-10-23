using Lambda.Host.Middleware;

namespace Lambda.Host;

public delegate Task LambdaMiddlewareDelegate(
    ILambdaHostContext context,
    LambdaInvocationDelegate next
);
