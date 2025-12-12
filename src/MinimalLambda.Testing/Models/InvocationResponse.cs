namespace MinimalLambda.Testing;

/// <summary>
/// Represents the result of a Lambda function invocation, containing either a successful response
/// or error information.
/// </summary>
/// <typeparam name="TResponse">The expected type of the successful Lambda response.</typeparam>
/// <remarks>
/// <para>
/// Use the <see cref="WasSuccess"/> property to determine whether the invocation succeeded or failed.
/// If successful, <see cref="Response"/> will contain the deserialized response data.
/// If failed, <see cref="Error"/> will contain details about the error.
/// </para>
/// </remarks>
public class InvocationResponse<TResponse>
{
    /// <summary>
    /// Gets the error information if the Lambda function invocation failed, or <see langword="null"/>
    /// if the invocation succeeded.
    /// </summary>
    /// <value>
    /// An <see cref="ErrorResponse"/> containing error details, or <see langword="null"/> for successful invocations.
    /// </value>
    /// <remarks>
    /// This property is populated when the Lambda function reports an error via the runtime API's
    /// error endpoint, or when the invocation times out or encounters other failures.
    /// </remarks>
    public ErrorResponse? Error { get; internal set; }

    /// <summary>
    /// Gets the Lambda function's response data if the invocation succeeded, or the default value
    /// of <typeparamref name="TResponse"/> if the invocation failed.
    /// </summary>
    /// <value>
    /// The deserialized response from the Lambda function, or <see langword="null"/> for failed invocations.
    /// </value>
    /// <remarks>
    /// This property is populated when the Lambda function successfully completes and returns a response
    /// via the runtime API's response endpoint.
    /// </remarks>
    public TResponse? Response { get; internal set; }

    /// <summary>
    /// Gets a value indicating whether the Lambda function invocation completed successfully.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the invocation succeeded and <see cref="Response"/> contains valid data;
    /// <see langword="false"/> if the invocation failed and <see cref="Error"/> contains error information.
    /// </value>
    /// <remarks>
    /// Use this property to determine which of <see cref="Response"/> or <see cref="Error"/> contains
    /// meaningful data for the invocation result.
    /// </remarks>
    public bool WasSuccess { get; internal set; }
}
