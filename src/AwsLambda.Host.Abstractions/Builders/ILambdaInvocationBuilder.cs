namespace AwsLambda.Host.Builder;

/// <summary>Builder for composing Lambda invocation handlers with middleware.</summary>
/// <remarks>
///     <para>
///         The <see cref="ILambdaInvocationBuilder" /> provides a fluent API to build a Lambda
///         invocation pipeline. Register a single handler using <see cref="Handle" />, add middleware
///         using <see cref="Use" />, and then call <see cref="Build" /> to create the final composed
///         <see cref="LambdaInvocationDelegate" />.
///     </para>
///     <para>
///         Middleware is applied in the order it is registered, with invocations flowing through
///         each middleware sequentially before reaching the handler.
///     </para>
/// </remarks>
public interface ILambdaInvocationBuilder
{
    /// <summary>Gets the currently registered handler, if any.</summary>
    LambdaInvocationDelegate? Handler { get; }

    /// <summary>Gets the list of registered middleware handlers.</summary>
    IReadOnlyList<Func<LambdaInvocationDelegate, LambdaInvocationDelegate>> Middlewares { get; }

    /// <summary>Gets a dictionary for storing state that is shared across invocations.</summary>
    IDictionary<string, object?> Properties { get; }

    /// <summary>Gets the service provider for dependency injection.</summary>
    IServiceProvider Services { get; }

    /// <summary>Registers the core handler for this invocation pipeline.</summary>
    /// <remarks>
    ///     <para>
    ///         Only one handler can be registered. Calling this method multiple times will throw an
    ///         <see cref="InvalidOperationException" />.
    ///     </para>
    /// </remarks>
    /// <param name="handler">The <see cref="LambdaInvocationDelegate" /> to handle invocations.</param>
    /// <returns>The current <see cref="ILambdaInvocationBuilder" /> instance for method chaining.</returns>
    ILambdaInvocationBuilder Handle(LambdaInvocationDelegate handler);

    /// <summary>Adds middleware to the invocation pipeline.</summary>
    /// <remarks>
    ///     <para>
    ///         Middleware is applied in the order it is registered. Each middleware receives the next
    ///         delegate in the pipeline and can intercept, transform, or handle the invocation before
    ///         passing it along.
    ///     </para>
    /// </remarks>
    /// <param name="middleware">
    ///     A function that receives the next <see cref="LambdaInvocationDelegate" />
    ///     and returns a new delegate with middleware behavior applied.
    /// </param>
    /// <returns>The current <see cref="ILambdaInvocationBuilder" /> instance for method chaining.</returns>
    ILambdaInvocationBuilder Use(
        Func<LambdaInvocationDelegate, LambdaInvocationDelegate> middleware
    );

    /// <summary>Builds the final invocation delegate by composing the handler and middleware.</summary>
    /// <remarks>
    ///     <para>
    ///         Composes the registered middleware with the handler so invocations flow through them in
    ///         the order they were registered. A handler must be registered before calling this method.
    ///     </para>
    /// </remarks>
    /// <returns>The composed <see cref="LambdaInvocationDelegate" /> ready for invocation.</returns>
    LambdaInvocationDelegate Build();
}
