using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.RuntimeSupport.Bootstrap;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.Serialization.SystemTextJson.Converters;

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

    /// <summary>Gets or sets the options for configuring the Lambda bootstrap behavior.</summary>
    /// <remarks>
    ///     <para>
    ///         The <see cref="LambdaBootstrapOptions" /> contains settings that control how the Lambda
    ///         runtime bootstrap process operates, including interaction with the Lambda runtime API.
    ///     </para>
    /// </remarks>
    public LambdaBootstrapOptions BootstrapOptions { get; set; } = new();

    /// <summary>
    ///     Gets or sets the JSON serializer options used for serializing and deserializing Lambda
    ///     requests and responses.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Default values are provided by <see cref="DefaultJsonSerializerOptions" />, but this can
    ///         be overridden to customize serialization behavior. Default <see cref="JsonConverter" />
    ///         used for different types can be overridden by registering your own custom converters.
    ///     </para>
    ///     <para>
    ///         When these options are used with the <see cref="DefaultLambdaHostJsonSerializer" />, the
    ///         <see cref="System.Text.Json.JsonSerializerOptions.PropertyNamingPolicy" /> is wrapped with
    ///         an instance of <see cref="AwsNamingPolicy" /> if it is not already an instance of that
    ///         type.
    ///     </para>
    /// </remarks>
    public JsonSerializerOptions JsonSerializerOptions { get; set; } =
        DefaultJsonSerializerOptions();

    /// <summary>
    ///     Gets or sets the JSON writer options used for serializing and deserializing Lambda
    ///     requests and responses.
    /// </summary>
    public JsonWriterOptions JsonWriterOptions { get; set; } = DefaultJsonWriterOptions();

    /// <summary>Gets or sets an optional custom HTTP client for the Lambda bootstrap.</summary>
    /// <remarks>
    ///     When null, the bootstrap will create its own HTTP client for communicating with the Lambda
    ///     runtime API. Provide a custom client to control connection pooling, timeout settings, or other
    ///     HTTP-level behaviors.
    /// </remarks>
    public HttpClient? BootstrapHttpClient { get; set; } = null;

    /// <summary>Creates default JSON serializer options for Lambda serialization.</summary>
    /// <returns>
    ///     A configured <see cref="System.Text.Json.JsonSerializerOptions" /> instance with AWS
    ///     conventions.
    /// </returns>
    private static JsonSerializerOptions DefaultJsonSerializerOptions()
    {
        var serializationOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = new AwsNamingPolicy(),
        };

        serializationOptions.Converters.Add(new DateTimeConverter());
        serializationOptions.Converters.Add(new MemoryStreamConverter());
        serializationOptions.Converters.Add(new ConstantClassConverter());
        serializationOptions.Converters.Add(new ByteArrayConverter());

        return serializationOptions;
    }

    /// <summary>Creates default JSON writer options for Lambda serialization.</summary>
    /// <returns>
    ///     A configured <see cref="System.Text.Json.JsonWriterOptions" /> instance with relaxed JSON
    ///     escaping.
    /// </returns>
    private static JsonWriterOptions DefaultJsonWriterOptions() =>
        new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
}
