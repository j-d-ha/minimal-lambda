using AwsLambda.Host.Middleware;

namespace AwsLambda.Host;

public static class HandlerLambdaApplicationExtensions
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

        application.Map(lambdaInvocationDelegate, null, null);

        return application;
    }
}
