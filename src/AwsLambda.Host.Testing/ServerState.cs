namespace AwsLambda.Host.Testing;

/// <summary>
/// Represents the lifecycle state of a LambdaTestServer.
/// </summary>
public enum ServerState
{
    /// <summary>
    /// Server created but not started.
    /// </summary>
    Created,

    /// <summary>
    /// Server is starting (building host).
    /// </summary>
    Starting,

    /// <summary>
    /// Server is running and accepting invocations.
    /// </summary>
    Running,

    /// <summary>
    /// Server is stopping.
    /// </summary>
    Stopping,

    /// <summary>
    /// Server has stopped cleanly.
    /// </summary>
    Stopped,

    /// <summary>
    /// Server has been disposed.
    /// </summary>
    Disposed,
}
