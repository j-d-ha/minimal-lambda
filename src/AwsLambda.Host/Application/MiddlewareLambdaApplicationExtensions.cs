namespace AwsLambda.Host;

public static class MiddlewareLambdaApplicationExtensions
{
    public static ILambdaApplication UseMiddleware(
        this ILambdaApplication application,
        Func<ILambdaHostContext, LambdaInvocationDelegate, Task> middleware
    ) =>
        application.Use(next =>
        {
            return context =>
            {
                return middleware(context, next);
            };
        });
}
