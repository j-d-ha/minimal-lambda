using System.Diagnostics;

namespace MinimalLambda.Example.OpenTelemetry;

internal class Service(Instrumentation instrumentation, NameMetrics nameMetrics) : IService
{
    private readonly ActivitySource _activitySource = instrumentation.ActivitySource;

    public async Task<string> GetMessage(string name, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity();

        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

        nameMetrics.ProcessName(name);

        return $"Hello {name}!";
    }
}
