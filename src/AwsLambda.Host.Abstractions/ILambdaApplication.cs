using Amazon.Lambda.Core;

namespace AwsLambda.Host;

public interface ILambdaApplication
{
    IServiceProvider Services { get; }

    ILambdaApplication Map(
        LambdaInvocationDelegate handler,
        Func<ILambdaHostContext, ILambdaSerializer, Stream, Task>? deserializer,
        Func<ILambdaHostContext, ILambdaSerializer, Task<Stream>>? serializer
    );

    ILambdaApplication Use(Func<LambdaInvocationDelegate, LambdaInvocationDelegate> middleware);

    /// <summary>
    ///     Registers a callback handler to be invoked when the AWS Lambda runtime initiates shutdown.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Multiple handlers can be registered and execute asynchronously using <see cref="Task.WhenAll(System.Collections.Generic.IEnumerable{Task})"/>.
    ///         Exceptions are collected and rethrown as <see cref="AggregateException"/>.
    ///     </para>
    ///     <para>
    ///         Each handler receives a scoped <see cref="IServiceProvider"/> for dependency resolution.
    ///         The <see cref="CancellationToken"/> expires before the Lambda runtime's hard timeout.
    ///         See <see cref="ShutdownDuration"/> for timing details.
    ///     </para>
    ///     <para>
    ///         Overloads exist to accept generic handler functions with up to 16 dependencies,
    ///         which are automatically resolved and injected. Source generation creates the wiring
    ///         code and compile-time interceptors to replace the calls at compile time.
    ///     </para>
    /// </remarks>
    /// <param name="handler">
    ///     A <see cref="LambdaShutdownDelegate"/> to invoke during shutdown.
    /// </param>
    /// <returns>
    ///     The current <see cref="ILambdaApplication"/> instance for method chaining.
    /// </returns>
    /// <seealso cref="LambdaShutdownDelegate"/>
    /// <seealso cref="ShutdownDuration"/>
    ILambdaApplication OnShutdown(LambdaShutdownDelegate handler);
}
