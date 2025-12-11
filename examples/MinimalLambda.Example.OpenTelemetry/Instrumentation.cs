using System.Diagnostics;

namespace MinimalLambda.Example.OpenTelemetry;

/// <summary>
///     It is recommended to use a custom type to hold references for ActivitySource. This avoids
///     possible type collisions with other components in the DI container.
/// </summary>
internal class Instrumentation : IDisposable
{
    internal const string ActivitySourceName = "MyLambda";
    internal const string ActivitySourceVersion = "1.0.0";

    internal ActivitySource ActivitySource { get; } =
        new(ActivitySourceName, ActivitySourceVersion);

    public void Dispose() => ActivitySource.Dispose();
}
