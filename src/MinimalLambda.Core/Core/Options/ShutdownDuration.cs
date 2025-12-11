namespace MinimalLambda.Host.Options;

/// <summary>
///     Provides predetermined timeout durations for AWS Lambda shutdown based on extension
///     availability.
/// </summary>
/// <remarks>
///     These constants represent the time window between when AWS sends SIGTERM and SIGKILL to a
///     Lambda function. The duration varies depending on whether Lambda extensions are enabled and
///     their type. Use these values to configure <see cref="LambdaHostOptions.ShutdownDuration" />.
/// </remarks>
public static class ShutdownDuration
{
    /// <summary>Gets the shutdown duration when no Lambda extensions are available (0ms).</summary>
    public static readonly TimeSpan NoExtensions = TimeSpan.Zero;

    /// <summary>Gets the shutdown duration when only internal Lambda extensions are enabled (300ms).</summary>
    public static readonly TimeSpan InternalExtensions = TimeSpan.FromMilliseconds(300);

    /// <summary>Gets the shutdown duration when external Lambda extensions are enabled (500ms).</summary>
    public static readonly TimeSpan ExternalExtensions = TimeSpan.FromMilliseconds(500);
}
