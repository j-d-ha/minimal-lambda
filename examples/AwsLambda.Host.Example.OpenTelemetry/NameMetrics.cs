using System.Diagnostics.Metrics;

namespace AwsLambda.Host.Example.OpenTelemetry;

public class NameMetrics
{
    private readonly Counter<int> _namesProcessed;

    public NameMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("MyLambda.Service");
        _namesProcessed = meter.CreateCounter<int>("MyLambda.Service.Processed");
    }

    public void ProcessName(string name) =>
        _namesProcessed.Add(1, new KeyValuePair<string, object?>("name", name));
}
