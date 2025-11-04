using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport.Bootstrap;
using Amazon.Lambda.Serialization.SystemTextJson;

namespace AwsLambda.Host;

/// <summary>Options for configuring Lambda hosting behavior.</summary>
public class LambdaHostOptions
{
    /// <summary>
    ///     Gets or sets the buffer duration subtracted from the Lambda function's remaining
    ///     invocation time when creating cancellation tokens.
    /// </summary>
    /// <remarks>Default is 3 seconds.</remarks>
    public TimeSpan InvocationCancellationBuffer { get; set; } = TimeSpan.FromSeconds(3);

    /// <summary>Gets or sets the timeout duration for <see cref="ILambdaApplication.OnInit" /> handlers.</summary>
    /// <remarks>
    ///     This value is used as the duration of the <see cref="CancellationTokenSource" /> that is
    ///     passed to <see cref="ILambdaApplication.OnInit" /> handlers. Default is 5 seconds.
    /// </remarks>
    public TimeSpan InitTimeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    ///     Gets or sets the duration between when AWS sends SIGTERM and SIGKILL to the Lambda
    ///     function.
    /// </summary>
    /// <remarks>
    ///     The <see cref="AwsLambda.Host.ShutdownDuration" /> class provides predetermined values for
    ///     common scenarios:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <see cref="ShutdownDuration.NoExtensions" /> (0ms) - No extension time
    ///                 available
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ShutdownDuration.InternalExtensions" /> (300ms) - Internal
    ///                 extensions only
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ShutdownDuration.ExternalExtensions" /> (500ms) - External
    ///                 extensions
    ///             </description>
    ///         </item>
    ///     </list>
    ///     Default is <see cref="ShutdownDuration.ExternalExtensions" />.
    /// </remarks>
    public TimeSpan ShutdownDuration { get; set; } = Host.ShutdownDuration.ExternalExtensions;

    /// <summary>
    ///     Gets or sets the buffer duration subtracted from <see cref="ShutdownDuration" /> to ensure
    ///     cleanup tasks complete.
    /// </summary>
    /// <remarks>
    ///     This buffer guarantees that cancellation tokens are fired before the Lambda container
    ///     exits, providing sufficient time for all cleanup tasks to execute. The actual timeout for
    ///     graceful shutdown is calculated as <see cref="ShutdownDuration" /> minus this buffer. Default
    ///     is 50 milliseconds.
    /// </remarks>
    public TimeSpan ShutdownDurationBuffer { get; set; } = TimeSpan.FromMilliseconds(50);

    /// <summary>Gets or sets the Lambda bootstrap options used to configure the AWS Lambda runtime.</summary>
    /// <remarks>
    ///     Defaults to a new instance of <see cref="LambdaBootstrapOptions" /> with default settings.
    ///     These options control how the Lambda runtime bootstrap behaves and processes incoming
    ///     invocations.
    /// </remarks>
    public bool ClearLambdaOutputFormatting { get; set; } = false;

    public LambdaBootstrapOptions BootstrapOptions { get; set; } = new();

    /// <summary>
    ///     Gets or sets the Lambda serializer used for serializing and deserializing Lambda requests
    ///     and responses.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="DefaultLambdaJsonSerializer" /> which uses System.Text.Json for
    ///     serialization. This serializer is used for all JSON serialization operations within the Lambda
    ///     host.
    /// </remarks>
    public ILambdaSerializer LambdaSerializer { get; set; } = new DefaultLambdaJsonSerializer();

    /// <summary>Gets or sets an optional custom HTTP client for the Lambda bootstrap.</summary>
    /// <remarks>
    ///     When null, the bootstrap will create its own HTTP client for communicating with the Lambda
    ///     runtime API. Provide a custom client to control connection pooling, timeout settings, or other
    ///     HTTP-level behaviors.
    /// </remarks>
    public HttpClient? BootstrapHttpClient { get; set; } = null;
}
