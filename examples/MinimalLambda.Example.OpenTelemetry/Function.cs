using MinimalLambda.Builder;

namespace MinimalLambda.Example.OpenTelemetry;

internal static class Function
{
    internal static async Task<Response> Handler(
        [FromEvent] Request request,
        IService service,
        Instrumentation instrumentation,
        CancellationToken cancellationToken
    )
    {
        using var activity = instrumentation.ActivitySource.StartActivity();

        var message = await service.GetMessage(request.Name, cancellationToken);

        return new Response(message, DateTime.UtcNow);
    }
}
