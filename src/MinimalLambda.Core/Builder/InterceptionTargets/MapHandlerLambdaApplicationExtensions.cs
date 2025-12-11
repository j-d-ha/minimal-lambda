using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace MinimalLambda.Host.Builder;

/// <summary>
///     Overloads for
///     <see
///         cref="ILambdaInvocationBuilder.Handle(LambdaInvocationDelegate)" />
///     that support automatic dependency injection and serialization for Lambda handlers.
/// </summary>
[ExcludeFromCodeCoverage]
public static class MapHandlerLambdaApplicationExtensions
{
    /// <summary>
    ///     Registers a Lambda invocation handler with automatic dependency injection and
    ///     serialization.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Source generation creates the wiring code to resolve handler dependencies, using
    ///         compile-time interceptors to replace this call. Handler parameters are automatically
    ///         injected from the dependency injection container with proper scoping. Serialization and
    ///         deserialization are handled automatically based on the handler's parameter and return
    ///         types.
    ///     </para>
    /// </remarks>
    /// <param name="application">
    ///     The <see cref="ILambdaInvocationBuilder" /> instance to register the
    ///     handler with.
    /// </param>
    /// <param name="handler">
    ///     A handler function that will be intercepted and replaced at compile time by
    ///     the source generator.
    /// </param>
    /// <returns>The current <see cref="ILambdaInvocationBuilder" /> instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if called at runtime; this exception is
    ///     unreachable as this method is intercepted by the source generator code at compile time.
    /// </exception>
    /// <seealso
    ///     cref="ILambdaInvocationBuilder.Handle(LambdaInvocationDelegate)" />
    public static ILambdaInvocationBuilder MapHandler(
        this ILambdaInvocationBuilder application,
        Delegate handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }
}
