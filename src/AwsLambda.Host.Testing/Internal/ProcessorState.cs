namespace AwsLambda.Host.Testing;

/// <summary>
/// Represents the lifecycle state of the InvocationProcessor.
/// </summary>
internal enum ProcessorState
{
    /// <summary>
    /// Processor has been created but not yet started.
    /// </summary>
    Created,

    /// <summary>
    /// Processor is running and waiting for initialization to complete.
    /// Transitions to Running on first successful /next request or to Stopped on init error.
    /// </summary>
    Initializing,

    /// <summary>
    /// Processor is running and processing invocations.
    /// Initialization completed successfully.
    /// </summary>
    Running,

    /// <summary>
    /// Processor is shutting down.
    /// Transitions to Stopped when shutdown completes.
    /// </summary>
    Stopping,

    /// <summary>
    /// Processor has stopped and is no longer processing requests.
    /// </summary>
    Stopped,
}
