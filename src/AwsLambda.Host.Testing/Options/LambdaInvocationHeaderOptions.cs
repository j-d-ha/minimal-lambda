namespace AwsLambda.Host.Testing;

/// <summary>
/// Headers returned in Lambda runtime API invocation responses.
/// </summary>
public class LambdaInvocationHeaderOptions
{
    /// <summary>
    /// Gets or sets additional custom headers to include in the Lambda invocation response.
    /// Use this to add any additional headers beyond the standard Lambda runtime headers.
    /// </summary>
    public Dictionary<string, string> AdditionalHeaders { get; set; } = new();

    /// <summary>
    /// Gets or sets the response date header.
    /// Maps to the <c>Date</c> HTTP header. Defaults to current UTC time.
    /// </summary>
    public DateTime Date { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the ARN of the Lambda function being invoked.
    /// Maps to the <c>Lambda-Runtime-Invoked-Function-Arn</c> header.
    /// </summary>
    public string FunctionArn { get; set; } =
        "arn:aws:lambda:us-west-2:123412341234:function:Function";

    /// <summary>
    /// Gets or sets the Lambda function timeout duration.
    /// Maps to the <c>Lambda-Runtime-Deadline-Ms</c> header as Unix epoch milliseconds.
    /// This determines when the Lambda function will timeout, calculated as the current time plus this duration.
    /// Defaults to 15 minutes.
    /// </summary>
    public TimeSpan FunctionTimeout { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Gets or sets the AWS request ID for this invocation.
    /// Maps to the <c>Lambda-Runtime-Aws-Request-Id</c> header.
    /// </summary>
    public string RequestId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the AWS X-Ray trace ID for distributed tracing.
    /// Maps to the <c>Lambda-Runtime-Trace-Id</c> header.
    /// </summary>
    public string TraceId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets whether to use chunked transfer encoding.
    /// Maps to the <c>Transfer-Encoding: chunked</c> HTTP header. Defaults to true.
    /// </summary>
    public bool TransferEncodingChunked { get; set; } = true;
}
