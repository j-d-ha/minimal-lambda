using System.Text.Json.Serialization;

namespace AwsLambda.Host.Testing;

/// <summary>
/// Represents an error response with type, message, stack trace, and optional cause.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// The underlying cause of this error, if any.
    /// </summary>
    [JsonPropertyName("cause")]
    public ErrorCause? Cause { get; set; }

    [JsonPropertyName("causes")]
    public List<ErrorCause>? Causes { get; set; }

    /// <summary>
    /// The error message describing what went wrong.
    /// </summary>
    [JsonPropertyName("errorMessage")]
    public required string ErrorMessage { get; set; }

    /// <summary>
    /// The type of error that occurred.
    /// </summary>
    [JsonPropertyName("errorType")]
    public required string ErrorType { get; set; }

    /// <summary>
    /// The stack trace showing where the error occurred.
    /// </summary>
    [JsonPropertyName("stackTrace")]
    public List<string> StackTrace { get; set; } = [];

    /// <summary>
    /// Represents the cause of an error, which can have its own nested cause.
    /// </summary>
    public class ErrorCause
    {
        /// <summary>
        /// The underlying cause of this error, if any.
        /// </summary>
        [JsonPropertyName("cause")]
        public ErrorCause? Cause { get; set; }

        /// <summary>
        /// The error message describing what went wrong.
        /// </summary>
        [JsonPropertyName("errorMessage")]
        public required string ErrorMessage { get; set; }

        /// <summary>
        /// The type of error that occurred.
        /// </summary>
        [JsonPropertyName("errorType")]
        public string ErrorType { get; set; }

        /// <summary>
        /// The stack trace showing where the error occurred.
        /// </summary>
        [JsonPropertyName("stackTrace")]
        public List<string> StackTrace { get; set; } = [];
    }
}
