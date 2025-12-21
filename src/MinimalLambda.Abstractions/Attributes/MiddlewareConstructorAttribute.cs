namespace MinimalLambda.Builder;

/// <summary>Marks the constructor to use when a middleware class has multiple constructors.</summary>
/// <remarks>
///     <para>
///         By default, the source generator selects the constructor with the most parameters. Apply
///         this attribute to explicitly select a different constructor. Only one constructor per class
///         can use this attribute, otherwise a compile-time error (diagnostic LH0005) is raised.
///     </para>
/// </remarks>
/// <example>
///     <code>
///     internal class MyMiddleware : ILambdaMiddleware
///     {
///         public MyMiddleware(ILogger logger, IMetrics metrics)
///         {
///             // Constructor with most parameters (would be selected by default)
///         }
///
///         [MiddlewareConstructor]
///         public MyMiddleware([FromArguments] string config)
///         {
///             // Explicitly selected constructor
///         }
///
///         public Task InvokeAsync(ILambdaInvocationContext context, LambdaInvocationDelegate next)
///             => next(context);
///     }
///     </code>
/// </example>
[AttributeUsage(AttributeTargets.Constructor)]
public class MiddlewareConstructorAttribute : Attribute;
