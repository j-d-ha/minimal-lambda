using Amazon.Lambda.Core;

namespace AwsLambda.Host.Core;

/// <summary>
///     Provides a factory for creating cancellation token sources configured for AWS Lambda
///     invocations.
/// </summary>
/// <remarks>
///     <para>
///         Implementations of this interface create <see cref="CancellationTokenSource" /> instances
///         that are pre-configured with appropriate timeouts based on the Lambda execution context,
///         ensuring cancellation occurs well before the Lambda runtime's hard timeout.
///     </para>
/// </remarks>
public interface ILambdaCancellationFactory
{
    /// <summary>
    ///     Creates a new <see cref="CancellationTokenSource" /> configured for the given Lambda
    ///     context.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The returned cancellation token source will be automatically cancelled before the Lambda
    ///         function timeout specified in <paramref name="context" />, allowing for graceful shutdown
    ///         of operations.
    ///     </para>
    /// </remarks>
    /// <param name="context">
    ///     The <see cref="ILambdaContext" /> containing Lambda invocation information,
    ///     including timeout.
    /// </param>
    /// <returns>
    ///     A new <see cref="CancellationTokenSource" /> configured with an appropriate timeout for
    ///     the Lambda invocation.
    /// </returns>
    public CancellationTokenSource NewCancellationTokenSource(ILambdaContext context);
}
