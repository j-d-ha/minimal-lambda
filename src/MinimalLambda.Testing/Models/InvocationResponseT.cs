namespace MinimalLambda.Testing;

/// <summary>
/// Represents the result of a Lambda function invocation with a typed response, containing either
/// a successful response or error information.
/// </summary>
/// <typeparam name="TResponse">The expected type of the successful Lambda response.</typeparam>
/// <remarks>
/// <para>
/// This generic class extends <see cref="InvocationResponse"/> to provide strongly-typed access
/// to the Lambda function's response data. Use the <see cref="InvocationResponse.WasSuccess"/>
/// property to determine whether the invocation succeeded or failed. If successful,
/// <see cref="Response"/> will contain the deserialized response data. If failed,
/// <see cref="InvocationResponse.Error"/> will contain details about the error.
/// </para>
/// </remarks>
public class InvocationResponse<TResponse> : InvocationResponse
{
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
    public TResponse? Response { get; internal init; }
}
