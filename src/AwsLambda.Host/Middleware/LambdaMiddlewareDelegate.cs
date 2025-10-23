using AwsLambda.Host.Middleware;

namespace AwsLambda.Host;

public delegate Task LambdaMiddlewareDelegate(
    ILambdaHostContext context,
    LambdaInvocationDelegate next
);
