using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace AwsLambda.Host.Testing;

/// <summary>
/// Client for invoking Lambda functions in tests.
/// Provides a clean API that abstracts HTTP details.
/// </summary>
public class LambdaClient
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly LambdaClientOptions _lambdaClientOptions;
    private readonly LambdaTestServer _server;
    private int _requestCounter;

    internal LambdaClient(LambdaTestServer server, JsonSerializerOptions jsonSerializerOptions)
    {
        _server = server;
        _jsonSerializerOptions = jsonSerializerOptions;
        _lambdaClientOptions = new LambdaClientOptions();
    }

    /// <summary>
    /// Configures client options for invocation headers.
    /// </summary>
    public LambdaClient ConfigureOptions(Action<LambdaClientOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(configureOptions);

        configureOptions(_lambdaClientOptions);

        return this;
    }

    /// <summary>
    /// Invokes the Lambda function with the given event and waits for the response.
    /// </summary>
    public async Task<InvocationResponse<TResponse>> InvokeAsync<TResponse, TEvent>(
        TEvent invokeEvent,
        CancellationToken cancellationToken = default
    )
    {
        // Generate unique request ID
        var requestId = GetRequestId();

        // Create the event response with Lambda headers
        var eventResponse = CreateEventResponse(invokeEvent, requestId);
        var deadlineUtc = DateTimeOffset.UtcNow.Add(
            _lambdaClientOptions.InvocationHeaderOptions.InvocationTimeout
        );

        // Queue invocation and wait for Bootstrap to process it
        using var timeoutCts = new CancellationTokenSource(
            _lambdaClientOptions.InvocationHeaderOptions.InvocationTimeout
        );
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            timeoutCts.Token
        );

        var completion = await _server.QueueInvocationAsync(
            requestId,
            eventResponse,
            deadlineUtc,
            linkedCts.Token
        );

        var responseMessage = completion.Request;
        var wasSuccess = completion.RequestType == RequestType.PostResponse;

        var invocationResponse = new InvocationResponse<TResponse>
        {
            WasSuccess = wasSuccess,
            Response = wasSuccess
                ? await (
                    responseMessage.Content?.ReadFromJsonAsync<TResponse>(
                        _jsonSerializerOptions,
                        cancellationToken
                    ) ?? Task.FromResult<TResponse?>(default)
                )
                : default,
            Error = !wasSuccess
                ? await (
                    responseMessage.Content?.ReadFromJsonAsync<ErrorResponse>(
                        _jsonSerializerOptions,
                        cancellationToken
                    ) ?? Task.FromResult<ErrorResponse?>(null)
                )
                : null,
        };

        return invocationResponse;
    }

    private HttpResponseMessage CreateEventResponse<TEvent>(TEvent invokeEvent, string requestId)
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
        response.Headers.Add("Lambda-Runtime-Aws-Request-Id", requestId);
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

    private string GetRequestId() =>
        Interlocked.Increment(ref _requestCounter).ToString().PadLeft(12, '0');
}
