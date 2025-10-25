using AwsLambda.Host.Middleware;

namespace AwsLambda.Host;

public static class MiddlewareLambdaApplicationExtensions
{
    public static ILambdaApplication UseMiddleware(
        this ILambdaApplication application,
        Func<ILambdaHostContext, LambdaInvocationDelegate, Task> middleware
    ) => application.Use(next => context => middleware(context, next));
}
