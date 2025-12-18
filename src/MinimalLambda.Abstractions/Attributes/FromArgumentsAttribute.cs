namespace MinimalLambda.Builder;

/// <summary>
///     Marks a middleware constructor parameter to receive its value from the arguments passed to
///     <c>UseMiddleware&lt;T&gt;()</c>.
/// </summary>
/// <remarks>
///     <para>
///         Parameters marked with this attribute are resolved exclusively from the <c>args</c> array
///         passed to <c>UseMiddleware&lt;T&gt;(params object[] args)</c>. If no matching argument is
///         found, an <see cref="InvalidOperationException" /> is thrown. Without this attribute,
///         parameters first attempt resolution from args, then fall back to the DI container if no
///         match is found.
///     </para>
/// </remarks>
/// <example>
///     <code>
///     internal class MyMiddleware([FromArguments] string config) : ILambdaMiddleware
///     {
///         public Task InvokeAsync(ILambdaInvocationContext context, LambdaInvocationDelegate next)
///             => next(context);
///     }
///     </code>
/// </example>
[AttributeUsage(AttributeTargets.Parameter)]
public class FromArgumentsAttribute : Attribute;
