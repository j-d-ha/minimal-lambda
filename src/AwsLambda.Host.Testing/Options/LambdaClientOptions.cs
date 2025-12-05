namespace AwsLambda.Host.Testing;

/// <summary>
/// Configuration options for the Lambda test client.
/// </summary>
public class LambdaClientOptions
{
    /// <summary>
    /// Gets or sets the headers to include in Lambda invocation responses.
    /// </summary>
    public LambdaInvocationHeaderOptions InvocationHeaderOptions { get; set; } = new();
}
