using System.Net;
using System.Text;
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
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(invokeEvent, _jsonSerializerOptions),
                Encoding.UTF8,
                "application/json"
            ),
            Version = Version.Parse("1.1"),
        };
        // Add response headers
        response.Headers.Date = new DateTimeOffset(2025, 12, 4, 20, 40, 53, TimeSpan.Zero);
        response.Headers.TransferEncodingChunked = true;

        // Add custom Lambda headers
        response.Headers.Add("ambda-Runtime-Deadline-Ms", "1764881754010");
        response.Headers.Add("Lambda-Runtime-Aws-Request-Id", "000000000002");
        response.Headers.Add("Lambda-Runtime-Trace-Id", "2a159b6d-ca3c-4991-8533-c2b2a8da0640");
        response.Headers.Add(
            "Lambda-Runtime-Invoked-Function-Arn",
            "arn:aws:lambda:us-west-2:123412341234:function:Function"
        );

        return default;
    }
}

/// <summary>
/// Configuration options for the Lambda test client.
/// </summary>
public class LambdaClientOptions
{
    /// <summary>
    /// Gets or sets the headers to include in Lambda invocation responses.
    /// </summary>
    public LambdaInvocationHeaders InvocationHeaders { get; set; } = new();
}

/// <summary>
/// Headers returned in Lambda runtime API invocation responses.
/// </summary>
public class LambdaInvocationHeaders
{
    /// <summary>
    /// Gets or sets the response date header. Defaults to current UTC time.
    /// </summary>
    public DateTime Date { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets whether to use chunked transfer encoding. Defaults to true.
    /// </summary>
    public bool TransferEncodingChunked { get; set; } = true;

    /// <summary>
    /// Gets or sets the Lambda invocation deadline in milliseconds since Unix epoch.
    /// This indicates when the Lambda function will timeout.
    /// </summary>
    public long DeadlineMs { get; set; } =
        DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeMilliseconds();

    /// <summary>
    /// Gets or sets the AWS request ID for this invocation.
    /// </summary>
    public string RequestId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the AWS X-Ray trace ID for distributed tracing.
    /// </summary>
    public string TraceId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the ARN of the Lambda function being invoked.
    /// </summary>
    public string FunctionArn { get; set; } =
        "arn:aws:lambda:us-west-2:123412341234:function:Function";
}
