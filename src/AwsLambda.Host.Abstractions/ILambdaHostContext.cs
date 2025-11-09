using Amazon.Lambda.Core;

namespace AwsLambda.Host;

/// <summary>
///     Encapsulates the information about a Lambda invocation. It extends
///     <see cref="ILambdaContext" /> with additional properties.
/// </summary>
public interface ILambdaHostContext : ILambdaContext
{
    /// <summary>Gets or sets the deserialized Lambda event object.</summary>
    /// <remarks>
    ///     <para>
    ///         This property contains the event data passed to the Lambda handler, deserialized into an
    ///         object instance. The type depends on the event source and the deserialization logic.
    ///     </para>
    /// </remarks>
    object? Event { get; set; }

    /// <summary>Gets or sets the response object to be serialized back to the Lambda caller.</summary>
    /// <remarks>
    ///     <para>
    ///         This property holds the value that will be serialized and returned as the result of the
    ///         Lambda invocation. It is typically set by the handler function during invocation
    ///         processing.
    ///     </para>
    /// </remarks>
    object? Response { get; set; }

    /// <summary>
    ///     Gets or sets the <see cref="IServiceProvider" /> that provides access to the invocation's
    ///     service container.
    /// </summary>
    IServiceProvider ServiceProvider { get; set; }

    /// <summary>
    ///     Gets or sets a key/value collection that can be used to share data within the scope of
    ///     this invocation.
    /// </summary>
    IDictionary<object, object?> Items { get; set; }

    /// <summary>
    ///     Gets the <see cref="CancellationToken" /> that signals a Lambda invocation is being
    ///     cancelled.
    /// </summary>
    /// <remarks>
    ///     The cancellation token will also be cancelled if a SIGTERM signal is received, indicting
    ///     that the Lambda runtime is being terminated.
    /// </remarks>
    CancellationToken CancellationToken { get; }
}
