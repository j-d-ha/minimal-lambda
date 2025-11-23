using Amazon.Lambda.RuntimeSupport.Bootstrap;

namespace AwsLambda.Host.Options;

/// <summary>Options for configuring Lambda hosting behavior.</summary>
public class LambdaHostOptions
{
    /// <summary>Gets or sets an optional custom HTTP client for the Lambda bootstrap.</summary>
    /// <remarks>
    ///     When null, the bootstrap will create its own HTTP client for communicating with the Lambda
    ///     runtime API. Provide a custom client to control connection pooling, timeout settings, or other
    ///     HTTP-level behaviors.
    /// </remarks>
    public HttpClient? BootstrapHttpClient { get; set; } = null;

    /// <summary>Gets or sets the options for configuring the Lambda bootstrap behavior.</summary>
    /// <remarks>
    ///     <para>
    ///         The <see cref="LambdaBootstrapOptions" /> contains settings that control how the Lambda
    ///         runtime bootstrap process operates, including interaction with the Lambda runtime API.
    ///     </para>
    /// </remarks>
    public LambdaBootstrapOptions BootstrapOptions { get; set; } = new();

    /// <summary>
    ///     Gets or sets whether to clear Lambda runtime output formatting on application
    ///     initialization.
    /// </summary>
    /// <remarks>
    ///     When set to <c>true</c>, the
    ///     <see cref="OutputFormattingLambdaApplicationExtensions.OnInitClearLambdaOutputFormatting" />
    ///     extension method is automatically registered to run during application startup. This clears the
    ///     custom formatting applied by the Lambda runtime, allowing structured logging frameworks like
    ///     Serilog to output JSON without corruption. Default is <c>false</c>.
    /// </remarks>
    public bool ClearLambdaOutputFormatting { get; set; } = false;

    /// <summary>Gets or sets the timeout duration for <see cref="ILambdaOnInitBuilder.OnInit" /> handlers.</summary>
    /// <remarks>
    ///     This value is used as the duration of the <see cref="CancellationTokenSource" /> that is
    ///     passed to <see cref="ILambdaOnInitBuilder.OnInit" /> handlers. Default is 5 seconds.
    /// </remarks>
    public TimeSpan InitTimeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    ///     Gets or sets the buffer duration subtracted from the Lambda function's remaining
    ///     invocation time when creating cancellation tokens.
    /// </summary>
    /// <remarks>Default is 3 seconds.</remarks>
    public TimeSpan InvocationCancellationBuffer { get; set; } = TimeSpan.FromSeconds(3);

    /// <summary>
    ///     Gets or sets the duration between when AWS sends SIGTERM and SIGKILL to the Lambda
    ///     function.
    /// </summary>
    /// <remarks>
    ///     The <see cref="ShutdownDuration" /> class provides predetermined values for common scenarios:
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
    public TimeSpan ShutdownDuration { get; set; } = Options.ShutdownDuration.ExternalExtensions;

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
}
