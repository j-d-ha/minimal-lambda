namespace MinimalLambda.Testing;

/// <summary>
/// Represents the result of a Lambda function initialization attempt.
/// </summary>
public class InitResponse
{
    /// <summary>
    /// Gets the error information if initialization failed, or null if initialization succeeded.
    /// </summary>
    public ErrorResponse? Error { get; internal init; }

    /// <summary>
    /// Gets the status of the initialization attempt.
    /// </summary>
    public InitStatus InitStatus { get; internal init; }
}
