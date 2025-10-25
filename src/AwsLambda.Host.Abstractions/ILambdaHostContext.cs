using Amazon.Lambda.Core;

namespace AwsLambda.Host;

/// <summary>
///     Encapsulates the information about a Lambda invocation.
///     It extends <see cref="ILambdaContext" /> with additional properties.
/// </summary>
public interface ILambdaHostContext : ILambdaContext
{
    object? Event { get; set; }

    object? Response { get; set; }

    /// <summary>
    ///     Gets or sets the <see cref="IServiceProvider" /> that provides access to the invocation's
    ///     service container.
    /// </summary>
    IServiceProvider ServiceProvider { get; set; }

    /// <summary>
    ///     Gets or sets a key/value collection that can be used to share data within the scope of this
    ///     invocation.
    /// </summary>
    IDictionary<object, object?> Items { get; set; }

    /// <summary>
    ///     Gets the <see cref="CancellationToken" /> that signals a Lambda invocation is being cancelled.
    /// </summary>
    /// <remarks>
    ///     The cancellation token will also be cancelled if a SIGTERM signal is received, indicting
    ///     that the Lambda runtime is being terminated.
    /// </remarks>
    CancellationToken CancellationToken { get; }
}
