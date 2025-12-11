using Amazon.Lambda.Core;

namespace MinimalLambda;

/// <summary>
///     Encapsulates the information about a Lambda invocation. It extends
///     <see cref="ILambdaContext" /> with additional properties.
/// </summary>
public interface ILambdaHostContext : ILambdaContext
{
    /// <summary>
    ///     Gets the <see cref="CancellationToken" /> that signals a Lambda invocation is being
    ///     cancelled.
    /// </summary>
    /// <remarks>
    ///     The cancellation token will also be cancelled if a SIGTERM signal is received, indicting
    ///     that the Lambda runtime is being terminated.
    /// </remarks>
    CancellationToken CancellationToken { get; }

    /// <summary>
    ///     Gets the <see cref="IFeatureCollection" /> that provides access to features available
    ///     during the Lambda invocation.
    /// </summary>
    IFeatureCollection Features { get; }

    /// <summary>
    ///     Gets or sets a key/value collection that can be used to share data within the scope of
    ///     this invocation.
    /// </summary>
    IDictionary<object, object?> Items { get; }

    /// <summary>Gets or sets a key/value collection that can be used to share data between invocations.</summary>
    IDictionary<string, object?> Properties { get; }

    /// <summary>
    ///     Gets or sets the <see cref="IServiceProvider" /> that provides access to the invocation's
    ///     service container.
    /// </summary>
    IServiceProvider ServiceProvider { get; }
}
