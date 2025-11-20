namespace AwsLambda.Host;

/// <summary>Contains the raw request and response streams for a Lambda invocation.</summary>
/// <remarks>
///     <para>
///         <see cref="RawInvocationData" /> provides access to the underlying streams for the Lambda
///         invocation event and response. These streams allow for low-level, direct access to the raw
///         invocation data without deserialization.
///     </para>
/// </remarks>
public class RawInvocationData
{
    /// <summary>Gets the stream containing the raw Lambda invocation event data.</summary>
    public Stream Event { get; init; }

    /// <summary>Gets or sets the stream for writing the Lambda invocation response.</summary>
    /// <remarks>
    ///     The invocation response will be written to this stream unless a stream is returned from
    ///     the handler, in which case the default stream is replaced with the returned stream.
    /// </remarks>
    public Stream Response { get; set; }
}
