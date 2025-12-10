namespace AwsLambda.Host.Testing;

/// <summary>
/// Configuration options for the Lambda test client.
/// </summary>
public class LambdaServerOptions
{
    /// <summary>
    /// Gets or sets additional custom headers to include in the Lambda invocation response.
    /// Use this to add any additional headers beyond the standard Lambda runtime headers.
    /// </summary>
    public Dictionary<string, string> AdditionalHeaders { get; set; } = new();

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
}
