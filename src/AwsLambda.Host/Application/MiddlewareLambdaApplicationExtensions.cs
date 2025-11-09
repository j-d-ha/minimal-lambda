namespace AwsLambda.Host;

/// <summary>Provides extension methods for adding middleware to the Lambda invocation pipeline.</summary>
public static class MiddlewareLambdaApplicationExtensions
{
    /// <summary>Adds middleware to the Lambda invocation pipeline using a simplified signature.</summary>
    /// <remarks>
    ///     <para>
    ///         This extension method provides a simpler API compared to
    ///         <see cref="ILambdaApplication.Use" />. Middleware is applied in the order registered and
    ///         can intercept invocations before they reach the handler, or process the response after the
    ///         handler completes.
    ///     </para>
    /// </remarks>
    /// <param name="application">The <see cref="ILambdaApplication" /> instance to add the middleware to.</param>
    /// <param name="middleware">
    ///     A function that receives the <see cref="ILambdaHostContext" /> and the
    ///     next <see cref="LambdaInvocationDelegate" /> in the pipeline, and returns a <see cref="Task" />
    ///     representing the asynchronous operation.
    /// </param>
    /// <returns>The current <see cref="ILambdaApplication" /> instance for method chaining.</returns>
    /// <seealso cref="ILambdaApplication.Use(Func{LambdaInvocationDelegate, LambdaInvocationDelegate})" />
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
