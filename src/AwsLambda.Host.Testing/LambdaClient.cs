using System.Text.Json;
using System.Threading.Channels;

namespace AwsLambda.Host.Testing;

public class LambdaClient
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly Channel<HttpRequestMessage> _requestChanel;
    private readonly Channel<HttpResponseMessage> _responseChanel;
    private readonly ILambdaRuntimeRouteManager _routeManager;
    private bool _isBootstrappingComplete;

    internal LambdaClient(
        Channel<HttpRequestMessage> requestChanel,
        Channel<HttpResponseMessage> responseChanel,
        JsonSerializerOptions jsonSerializerOptions,
        ILambdaRuntimeRouteManager routeManager
    )
    {
        _requestChanel = requestChanel;
        _responseChanel = responseChanel;
        _jsonSerializerOptions = jsonSerializerOptions;
        _routeManager = routeManager;
    }

    internal async Task WaitForBootstrapAsync(CancellationToken cancellationToken = default)
    {
        if (_isBootstrappingComplete)
            return;

        var request = await WaitForRequestAsync(cancellationToken);

        if (request.RequestType != RequestType.GetNextInvocation)
            throw new InvalidOperationException(
                $"Unexpected request received during bootstrap: {request.RequestType.ToString()}"
            );

        _isBootstrappingComplete = true;
    }

    private async Task<LambdaBootstrapRequest> WaitForRequestAsync(
        CancellationToken cancellationToken = default
    )
    {
        var request = await _requestChanel.Reader.ReadAsync(cancellationToken);

        if (!_routeManager.TryMatch(request, out var routeType, out var routeValue))
            throw new InvalidOperationException(
                $"Unexpected request received: {request.Method} {request.RequestUri?.PathAndQuery ?? "(no URI)"}"
            );

        return new LambdaBootstrapRequest
        {
            RequestType = routeType!.Value,
            RequestMessage = request,
            RouteValue = routeValue!,
        };
    }

    public async Task<InvocationResponse<TResponse>> Invoke<TEvent, TResponse>(
        TEvent invokeEvent,
        CancellationToken cancellationToken = default
    )
    {
        var eventJson = JsonSerializer.Serialize(invokeEvent, _jsonSerializerOptions);

        return default;
    }
}
