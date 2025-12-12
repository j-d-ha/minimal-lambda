namespace MinimalLambda.Testing;

/// <summary>
/// Represents the lifecycle state of a LambdaTestServer.
/// </summary>
public enum ServerState
{
    /// <summary>
    /// TestServer created but not started.
    /// </summary>
    Created,

    /// <summary>
    /// TestServer is starting (building host).
    /// </summary>
    Starting,

    /// <summary>
    /// TestServer is running and accepting invocations.
    /// </summary>
    Running,

    /// <summary>
    /// TestServer is stopping.
    /// </summary>
    Stopping,

    /// <summary>
    /// TestServer has stopped cleanly.
    /// </summary>
    Stopped,

    /// <summary>
    /// TestServer has been disposed.
    /// </summary>
    Disposed,
}
