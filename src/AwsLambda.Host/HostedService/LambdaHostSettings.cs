using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport.Bootstrap;
using Amazon.Lambda.Serialization.SystemTextJson;

namespace AwsLambda.Host;

/// <summary>
///     Options for configuring Lambda hosting behavior.
/// </summary>
public class LambdaHostSettings
{
    /// <summary>
    ///     Gets or sets the buffer duration subtracted from the Lambda function's remaining
    ///     invocation time when creating cancellation tokens.
    /// </summary>
    /// <remarks>Default is 3 seconds.</remarks>
    public TimeSpan InvocationCancellationBuffer { get; set; } = TimeSpan.FromSeconds(3);

    /// <summary>
    ///     Gets or sets the Lambda serializer. If null, defaults to
    ///     <see cref="Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer" />.
    /// </summary>
    public LambdaBootstrapOptions BootstrapOptions { get; set; } = new();

    public ILambdaSerializer LambdaSerializer { get; set; } = new DefaultLambdaJsonSerializer();

    public HttpClient? BootstrapHttpClient { get; set; } = null;
}
