using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport.Bootstrap;
using Amazon.Lambda.Serialization.SystemTextJson;

namespace AwsLambda.Host;

/// <summary>
///     Options for configuring Lambda hosting behavior.
/// </summary>
public class LambdaHostOptions
{
    /// <summary>
    ///     Gets or sets the buffer duration subtracted from the Lambda function's remaining
    ///     invocation time when creating cancellation tokens.
    /// </summary>
    /// <remarks>Default is 3 seconds.</remarks>
    public TimeSpan InvocationCancellationBuffer { get; set; } = TimeSpan.FromSeconds(3);

    /// <summary>
    /// Gets or sets the Lambda bootstrap options used to configure the AWS Lambda runtime.
    /// </summary>
    /// <remarks>
    /// Defaults to a new instance of <see cref="LambdaBootstrapOptions"/> with default settings.
    /// These options control how the Lambda runtime bootstrap behaves and processes incoming invocations.
    /// </remarks>
    public LambdaBootstrapOptions BootstrapOptions { get; set; } = new();

    /// <summary>
    /// Gets or sets the Lambda serializer used for serializing and deserializing Lambda requests and responses.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="DefaultLambdaJsonSerializer"/> which uses System.Text.Json for serialization.
    /// This serializer is used for all JSON serialization operations within the Lambda host.
    /// </remarks>
    public ILambdaSerializer LambdaSerializer { get; set; } = new DefaultLambdaJsonSerializer();

    /// <summary>
    /// Gets or sets an optional custom HTTP client for the Lambda bootstrap.
    /// </summary>
    /// <remarks>
    /// When null, the bootstrap will create its own HTTP client for communicating with the Lambda runtime API.
    /// Provide a custom client to control connection pooling, timeout settings, or other HTTP-level behaviors.
    /// </remarks>
    public HttpClient? BootstrapHttpClient { get; set; } = null;
}
