using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace MinimalLambda.Builder;

/// <summary>
///     Provides extension methods for registering class-based middleware to the Lambda invocation
///     pipeline.
/// </summary>
[ExcludeFromCodeCoverage]
public static class UseMiddlewareLambdaApplicationExtensions
{
    /// <summary>Registers a class-based middleware component with automatic dependency injection.</summary>
    /// <remarks>
    ///     <para>
    ///         Source generation creates the wiring code to instantiate the middleware and resolve its
    ///         constructor parameters, using compile-time interceptors to replace this call. Constructor
    ///         parameters can be annotated with <see cref="FromArgumentsAttribute" /> to resolve from
    ///         <paramref name="args" />, <see cref="FromServicesAttribute" /> to resolve from the DI
    ///         container, or <c>FromKeyedServicesAttribute</c> for keyed services. Use
    ///         <see cref="MiddlewareConstructorAttribute" /> to select a specific constructor when multiple
    ///         exist.
    ///     </para>
    /// </remarks>
    /// <typeparam name="T">The middleware type implementing <see cref="ILambdaMiddleware" />.</typeparam>
    /// <param name="builder">
    ///     The <see cref="ILambdaInvocationBuilder" /> instance to register the middleware
    ///     with.
    /// </param>
    /// <param name="args">
    ///     Arguments to pass to the middleware constructor for parameters marked with
    ///     <see cref="FromArgumentsAttribute" />.
    /// </param>
    /// <returns>The current <see cref="ILambdaInvocationBuilder" /> instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if called at runtime; this exception is
    ///     unreachable as this method is intercepted by the source generator code at compile time.
    /// </exception>
    /// <seealso cref="ILambdaMiddleware" />
    /// <seealso cref="FromArgumentsAttribute" />
    /// <seealso cref="FromServicesAttribute" />
    /// <seealso cref="MiddlewareConstructorAttribute" />
    public static ILambdaInvocationBuilder UseMiddleware<T>(
        this ILambdaInvocationBuilder builder,
        params object[] args
    )
        where T : ILambdaMiddleware
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }
}
