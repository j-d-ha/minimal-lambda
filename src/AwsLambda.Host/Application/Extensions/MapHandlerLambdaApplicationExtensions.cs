using System.Diagnostics;
using Amazon.Lambda.Core;

namespace AwsLambda.Host;

/// <summary>
///     Overloads for
///     <see
///         cref="ILambdaApplication.MapHandler(LambdaInvocationDelegate, Func{ILambdaHostContext, ILambdaSerializer, Stream, Task}, Func{ILambdaHostContext, ILambdaSerializer, Task{Stream}})" />
///     that support automatic dependency injection and serialization for Lambda handlers.
/// </summary>
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
    ///     The <see cref="ILambdaApplication" /> instance to register the handler
    ///     with.
    /// </param>
    /// <param name="handler">
    ///     A handler function that will be intercepted and replaced at compile time by
    ///     the source generator.
    /// </param>
    /// <returns>The current <see cref="ILambdaApplication" /> instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if called at runtime; this exception is
    ///     unreachable as this method is intercepted by the source generator code at compile time.
    /// </exception>
    /// <seealso
    ///     cref="ILambdaApplication.MapHandler(LambdaInvocationDelegate, Func{ILambdaHostContext, ILambdaSerializer, Stream, Task}, Func{ILambdaHostContext, ILambdaSerializer, Task{Stream}})" />
    public static ILambdaHandlerBuilder MapHandler(
        this ILambdaHandlerBuilder application,
        Delegate handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }
}
