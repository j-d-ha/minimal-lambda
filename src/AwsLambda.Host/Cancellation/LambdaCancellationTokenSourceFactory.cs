using Amazon.Lambda.Core;

namespace AwsLambda.Host;

/// <summary>
///     Provides functionality to create a <see cref="CancellationTokenSource" /> with respect to
///     the remaining time from the AWS Lambda execution context. This factory ensures that
///     cancellation tokens are created with a buffer duration to provide sufficient time before the
///     Lambda execution timeout occurs.
/// </summary>
public class LambdaCancellationTokenSourceFactory : ILambdaCancellationTokenSourceFactory
{
    /// <summary>
    ///     Represents a time duration that is subtracted from the remaining time of a Lambda function
    ///     to create a buffer, ensuring sufficient time for graceful resource cleanup and exit processes.
    /// </summary>
    /// <remarks>
    ///     The buffer duration is used when determining the maximum allowed runtime duration for a
    ///     generated <see cref="CancellationTokenSource" />. If the buffer duration is greater than or
    ///     equal to the Lambda function's remaining execution time, an
    ///     <see cref="InvalidOperationException" /> will be thrown.
    /// </remarks>
    private readonly TimeSpan _bufferDuration;

    /// <summary>
    ///     Factory class responsible for creating and configuring instances of
    ///     <see cref="CancellationTokenSource" /> for AWS Lambda functions. The factory uses a buffer
    ///     duration to ensure that the created cancellation tokens expire in a timely manner before the
    ///     remaining execution time of the Lambda function is exhausted.
    /// </summary>
    public LambdaCancellationTokenSourceFactory(TimeSpan bufferDuration)
    {
        if (bufferDuration < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(
                nameof(bufferDuration),
                "bufferDuration must be greater than or equal to zero."
            );

        _bufferDuration = bufferDuration;
    }

    /// <summary>
    ///     Creates a new <see cref="CancellationTokenSource" /> for a Lambda function, considering
    ///     the remaining execution time and a buffer duration.
    /// </summary>
    /// <param name="context">
    ///     The AWS Lambda context providing information about the current invocation,
    ///     including the remaining execution time.
    /// </param>
    /// <returns>
    ///     A <see cref="CancellationTokenSource" /> configured to cancel after the remaining duration
    ///     of the Lambda execution time minus the defined buffer duration.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when the provided <paramref name="context" /> is
    ///     null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the Lambda context has no remaining time or
    ///     when the buffer duration exceeds the remaining time, making it impossible to create a valid
    ///     <see cref="CancellationTokenSource" />.
    /// </exception>
    public CancellationTokenSource NewCancellationTokenSource(ILambdaContext context)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));

        if (context.RemainingTime <= TimeSpan.Zero)
            throw new InvalidOperationException("Lambda context has no remaining time");

        var maxAllowedDuration = context.RemainingTime - _bufferDuration;
        if (maxAllowedDuration <= TimeSpan.Zero)
            throw new InvalidOperationException(
                "CancellationTokenSource provided with insufficient time. "
                    + $"Lambda Remaining Time = {context.RemainingTime:c}, "
                    + $"Cancellation Token Buffer = {_bufferDuration:c}, "
                    + $"Candidate Token Duration = {maxAllowedDuration:c}"
            );

        return new CancellationTokenSource(maxAllowedDuration);
    }
}
