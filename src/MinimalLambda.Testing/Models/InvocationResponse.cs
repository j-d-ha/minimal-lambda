namespace MinimalLambda.Testing;

/// <summary>
/// Represents the base result of a Lambda function invocation, containing success status and
/// error information.
/// </summary>
/// <remarks>
/// <para>
/// This class serves as the non-generic base for <see cref="InvocationResponse{TResponse}"/>,
/// providing common properties for all invocation results regardless of the response type.
/// It contains the <see cref="WasSuccess"/> flag and <see cref="Error"/> information that
/// are shared across all invocation responses.
/// </para>
/// <para>
/// For invocations that return typed response data, use the generic
/// <see cref="InvocationResponse{TResponse}"/> class instead.
/// </para>
/// </remarks>
public class InvocationResponse
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
    public ErrorResponse? Error { get; internal init; }

    /// <summary>
    /// Gets a value indicating whether the Lambda function invocation completed successfully.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the invocation succeeded and response data is available (in
    /// <see cref="InvocationResponse{TResponse}.Response"/> for generic invocations);
    /// <see langword="false"/> if the invocation failed and <see cref="Error"/> contains error information.
    /// </value>
    /// <remarks>
    /// Use this property to determine whether the invocation succeeded or failed, which indicates
    /// whether response data or <see cref="Error"/> information contains meaningful data for the
    /// invocation result.
    /// </remarks>
    public bool WasSuccess { get; internal init; }
}
