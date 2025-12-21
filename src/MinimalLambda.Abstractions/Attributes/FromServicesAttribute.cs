namespace MinimalLambda.Builder;

/// <summary>Marks a middleware constructor parameter to receive its value from the dependency injection container.</summary>
/// <remarks>
///     <para>
///         Parameters marked with this attribute are resolved exclusively from the DI container, skipping
///         argument resolution. Without this attribute, parameters first attempt resolution from args, then
///         fall back to the DI container if no match is found.
///     </para>
/// </remarks>
/// <example>
///     <code>
///     internal class MyMiddleware([FromServices] ILogger logger) : ILambdaMiddleware
///     {
///         public Task InvokeAsync(ILambdaInvocationContext context, LambdaInvocationDelegate next)
///             => next(context);
///     }
///     </code>
/// </example>
[AttributeUsage(AttributeTargets.Parameter)]
public class FromServicesAttribute : Attribute;
