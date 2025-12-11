namespace MinimalLambda.Example.OpenTelemetry;

internal interface IService
{
    Task<string> GetMessage(string name, CancellationToken cancellationToken = default);
}
