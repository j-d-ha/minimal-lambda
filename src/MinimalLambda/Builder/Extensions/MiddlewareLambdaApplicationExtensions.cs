namespace MinimalLambda.Builder;

/// <summary>Provides extension methods for adding middleware to the Lambda invocation pipeline.</summary>
public static class MiddlewareLambdaApplicationExtensions
{
    extension(ILambdaInvocationBuilder application)
    {
        /// <summary>Adds middleware to the Lambda invocation pipeline using a simplified signature.</summary>
        /// <remarks>
        ///     <para>
        ///         This extension method provides a simpler API compared to
        ///         <see cref="ILambdaInvocationBuilder.Use" />. Middleware is applied in the order registered
        ///         and can intercept invocations before they reach the handler, or process the response after
        ///         the handler completes.
        ///     </para>
        /// </remarks>
        /// <param name="middleware">
        ///     A function that receives the <see cref="ILambdaInvocationContext" /> and the
        ///     next <see cref="LambdaInvocationDelegate" /> in the pipeline, and returns a <see cref="Task" />
        ///     representing the asynchronous operation.
        /// </param>
        /// <returns>The current <see cref="ILambdaInvocationBuilder" /> instance for method chaining.</returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <see cref="ILambdaInvocationBuilder" /> is
        ///     <c>null</c>.
        /// </exception>
        /// <seealso cref="ILambdaInvocationBuilder.Use" />
        public ILambdaInvocationBuilder UseMiddleware(
            Func<ILambdaInvocationContext, LambdaInvocationDelegate, Task> middleware
        )
        {
            ArgumentNullException.ThrowIfNull(application);

            application.Use(next =>
            {
                return context =>
                {
                    return middleware(context, next);
                };
            });

            return application;
        }
    }
}
