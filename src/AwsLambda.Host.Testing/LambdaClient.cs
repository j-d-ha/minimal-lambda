using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace AwsLambda.Host.Testing;

public class LambdaClient
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly LambdaClientOptions _lambdaClientOptions;
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
        _lambdaClientOptions = new LambdaClientOptions();
    }

    public LambdaClient ConfigureOptions(Action<LambdaClientOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(configureOptions);

        configureOptions(_lambdaClientOptions);

        return this;
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

    private HttpResponseMessage CreateRequest<TEvent>(TEvent invokeEvent)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(invokeEvent, _jsonSerializerOptions),
                Encoding.UTF8,
                "application/json"
            ),
            Version = Version.Parse("1.1"),
        };

        // Add standard HTTP headers
        response.Headers.Date = new DateTimeOffset(
            _lambdaClientOptions.InvocationHeaderOptions.Date
        );
        response.Headers.TransferEncodingChunked = _lambdaClientOptions
            .InvocationHeaderOptions
            .TransferEncodingChunked;

        // Add custom Lambda runtime headers
        var deadlineMs = DateTimeOffset
            .UtcNow.Add(_lambdaClientOptions.InvocationHeaderOptions.FunctionTimeout)
            .ToUnixTimeMilliseconds();
        response.Headers.Add("Lambda-Runtime-Deadline-Ms", deadlineMs.ToString());
        response.Headers.Add(
            "Lambda-Runtime-Aws-Request-Id",
            _lambdaClientOptions.InvocationHeaderOptions.RequestId
        );
        response.Headers.Add(
            "Lambda-Runtime-Trace-Id",
            _lambdaClientOptions.InvocationHeaderOptions.TraceId
        );
        response.Headers.Add(
            "Lambda-Runtime-Invoked-Function-Arn",
            _lambdaClientOptions.InvocationHeaderOptions.FunctionArn
        );

        // Add any additional custom headers
        foreach (var header in _lambdaClientOptions.InvocationHeaderOptions.AdditionalHeaders)
            response.Headers.Add(header.Key, header.Value);

        return response;
    }

    public async Task<InvocationResponse<TResponse>> Invoke<TEvent, TResponse>(
        TEvent invokeEvent,
        CancellationToken cancellationToken = default
    )
    {
        var response = CreateRequest(invokeEvent);

        return default;
    }
}
