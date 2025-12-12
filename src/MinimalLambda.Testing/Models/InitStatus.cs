namespace MinimalLambda.Testing;

/// <summary>
/// An enumeration of possible statuses for Lambda initialization.
/// </summary>
public enum InitStatus
{
    /// <summary>
    /// Initialization of the Lambda completed successfully.
    /// </summary>
    InitCompleted,

    /// <summary>
    /// Initialization of the Lambda failed, and the Lambda returned an error.
    /// </summary>
    InitError,

    /// <summary>
    /// Initialization of the Lambda failed, and the Host process exited.
    /// </summary>
    HostExited,
}
