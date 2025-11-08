using Amazon.Lambda.Core;

namespace AwsLambda.Host;

public interface ILambdaApplication
{
    /// <summary>Gets the service provider for resolving dependencies.</summary>
    IServiceProvider Services { get; }

    /// <summary>
    ///     Registers a handler function to process Lambda invocations with custom deserialization and
    ///     serialization.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Only one handler can be registered. Attempting to register multiple handlers will result
    ///         in an error.
    ///     </para>
    ///     <para>
    ///         Extension method overloads exist to enable source generation to automatically handle
    ///         serialization, deserialization, and dependency injection. These overloads provide a more
    ///         convenient API for common scenarios and should be preferred when possible.
    ///     </para>
    /// </remarks>
    /// <param name="handler">
    ///     The <see cref="LambdaInvocationDelegate" /> handler function that processes
    ///     the Lambda invocation.
    /// </param>
    /// <param name="deserializer">
    ///     An optional function to deserialize the incoming Lambda event from a
    ///     stream. If <c>null</c>, the event in the context is expected to be a <see cref="Stream" /> and
    ///     no deserialization is applied.
    /// </param>
    /// <param name="serializer">
    ///     An optional function to serialize the handler response to a stream. If
    ///     <c>null</c>, the response in the context is expected to be a <see cref="Stream" /> and no
    ///     serialization is applied.
    /// </param>
    /// <returns>The current <see cref="ILambdaApplication" /> instance for method chaining.</returns>
    /// <seealso cref="LambdaInvocationDelegate" />
    /// <seealso cref="Use(Func{LambdaInvocationDelegate, LambdaInvocationDelegate})" />
    ILambdaApplication MapHandler(
        LambdaInvocationDelegate handler,
        Func<ILambdaHostContext, ILambdaSerializer, Stream, Task>? deserializer,
        Func<ILambdaHostContext, ILambdaSerializer, Task<Stream>>? serializer
    );

    ILambdaApplication Use(Func<LambdaInvocationDelegate, LambdaInvocationDelegate> middleware);

    /// <summary>
    ///     Registers a callback handler to be invoked during the AWS Lambda Function Init phase,
    ///     before any handler invocation.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Multiple handlers can be registered and execute asynchronously using
    ///         <see
    ///             cref="Task.WhenAll{TResult}(System.Collections.Generic.IEnumerable{System.Threading.Tasks.Task{TResult}})" />
    ///         . Exceptions are collected and rethrown as <see cref="AggregateException" />.
    ///     </para>
    ///     <para>
    ///         Each handler receives a scoped <see cref="IServiceProvider" /> for dependency resolution
    ///         and a <see cref="CancellationToken" /> to signal when the Function Init phase should be
    ///         cancelled. The Function Init phase continues only if all handlers return <c>true</c>. If
    ///         any handler returns <c>false</c>, the Function Init phase is aborted. See
    ///         <c>LambdaHostOptions</c> for timing details and configuration options.
    ///     </para>
    ///     <para>
    ///         This is useful when using AWS Lambda Snap Start to initialize resources and state during
    ///         the Function Init phase.
    ///     </para>
    ///     <para>
    ///         Overloads exist to accept generic handler functions with up to 16 dependencies, which are
    ///         automatically resolved and injected. Source generation creates the wiring code and
    ///         compile-time interceptors to replace the calls at compile time.
    ///     </para>
    /// </remarks>
    /// <param name="handler">A <see cref="LambdaInitDelegate" /> to invoke during the Function Init phase.</param>
    /// <returns>The current <see cref="ILambdaApplication" /> instance for method chaining.</returns>
    /// <seealso cref="LambdaInitDelegate" />
    ILambdaApplication OnInit(LambdaInitDelegate handler);

    /// <summary>Registers a callback handler to be invoked during the AWS Lambda Function Shutdown phase.</summary>
    /// <remarks>
    ///     <para>
    ///         Multiple handlers can be registered and execute asynchronously using
    ///         <see cref="Task.WhenAll(System.Collections.Generic.IEnumerable{Task})" />. Exceptions are
    ///         collected and rethrown as <see cref="AggregateException" />.
    ///     </para>
    ///     <para>
    ///         Each handler receives a scoped <see cref="IServiceProvider" /> for dependency resolution.
    ///         The <see cref="CancellationToken" /> expires before the Lambda runtime's hard timeout. See
    ///         <c>LambdaHostOptions</c> for timing details and configuration options.
    ///     </para>
    ///     <para>
    ///         Overloads exist to accept generic handler functions with up to 16 dependencies, which are
    ///         automatically resolved and injected. Source generation creates the wiring code and
    ///         compile-time interceptors to replace the calls at compile time.
    ///     </para>
    /// </remarks>
    /// <param name="handler">A <see cref="LambdaShutdownDelegate" /> to invoke during shutdown.</param>
    /// <returns>The current <see cref="ILambdaApplication" /> instance for method chaining.</returns>
    /// <seealso cref="LambdaShutdownDelegate" />
    ILambdaApplication OnShutdown(LambdaShutdownDelegate handler);
}
