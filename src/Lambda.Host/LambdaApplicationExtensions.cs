using Lambda.Host.Middleware;

namespace Lambda.Host;

public static class LambdaApplicationExtensions
{
    public static ILambdaApplication MapHandler(
        this ILambdaApplication application,
        Delegate handler
    )
    {
        if (handler is not LambdaInvocationDelegate lambdaInvocationDelegate)
            throw new ArgumentException(
                "Handler must be a LambdaInvocationDelegate",
                nameof(handler)
            );

        application.MapHandler(lambdaInvocationDelegate);

        return application;
    }
}
