namespace MinimalLambda;

/// <summary>Defines a middleware component for the Lambda invocation pipeline.</summary>
/// <remarks>
///     <para>
///         Middleware components are invoked in sequence during each Lambda invocation. Each middleware
///         can perform operations before and after calling the next delegate to continue the pipeline.
///         Register middleware using <c>UseMiddleware&lt;T&gt;()</c>. Constructor parameters can be
///         annotated with <see cref="FromArgumentsAttribute" />, <see cref="FromServicesAttribute" />,
///         or <c>FromKeyedServicesAttribute</c>. Use <see cref="MiddlewareConstructorAttribute" /> to
///         select a specific constructor when multiple exist.
///     </para>
/// </remarks>
public interface ILambdaMiddleware
{
    /// <summary>Processes a Lambda invocation.</summary>
    /// <param name="context">The <see cref="ILambdaInvocationContext" /> for the current invocation.</param>
    /// <param name="next">The delegate representing the next middleware in the pipeline.</param>
    /// <returns>A <see cref="Task" /> that completes when the middleware processing finishes.</returns>
    Task InvokeAsync(ILambdaInvocationContext context, LambdaInvocationDelegate next);
}
