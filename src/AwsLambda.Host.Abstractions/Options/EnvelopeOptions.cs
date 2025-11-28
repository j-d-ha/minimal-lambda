using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.Serialization.SystemTextJson.Converters;
using AwsLambda.Host.Envelopes;

namespace AwsLambda.Host.Options;

/// <summary>Options for configuring envelope payload serialization and deserialization.</summary>
/// <remarks>
///     These options are used by implementations of <see cref="IRequestEnvelope" /> and
///     <see cref="IResponseEnvelope" /> to control how Lambda event payloads are processed.
/// </remarks>
/// <seealso cref="IRequestEnvelope" />
/// <seealso cref="IResponseEnvelope" />
public class EnvelopeOptions
{
    /// <summary>
    ///     Gets the default JSON serialization options that match those used by
    ///     <see cref="DefaultLambdaJsonSerializer" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Provides AWS Lambda-specific JSON settings for deserialization. This is used for complex
    ///         envelope payloads such as SNS to SQS and CloudWatch Logs.
    ///     </para>
    ///     <para>
    ///         The <see cref="JsonSerializerOptions.TypeInfoResolver" /> from <see cref="JsonOptions" />
    ///         will be added to these options during post-configuration.
    ///     </para>
    /// </remarks>
    public readonly Lazy<JsonSerializerOptions> LambdaDefaultJsonOptions = new(() =>
    {
        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = new AwsNamingPolicy(),
        };
        options.Converters.Add(new DateTimeConverter());
        options.Converters.Add(new MemoryStreamConverter());
        options.Converters.Add(new ConstantClassConverter());
        options.Converters.Add(new ByteArrayConverter());

        return options;
    });

    /// <summary>
    ///     Gets or sets a dictionary for storing custom extension data associated with envelope
    ///     processing.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This dictionary allows envelope implementations to store and access custom context or
    ///         configuration data that may be needed during serialization or deserialization.
    ///     </para>
    ///     <para>Default is an empty dictionary.</para>
    /// </remarks>
    public Dictionary<object, object> Items { get; set; } = new();

    /// <summary>
    ///     Gets or sets the JSON serialization options used when extracting and packing Lambda event
    ///     payloads.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Custom converters, naming policies, and other JSON serialization settings can be
    ///         configured here.
    ///     </para>
    ///     <para>Default is an empty <see cref="JsonSerializerOptions" /> instance.</para>
    /// </remarks>
    public JsonSerializerOptions JsonOptions { get; set; } = new();

    /// <summary>Gets or sets the XML reader settings used when deserializing Lambda event payloads.</summary>
    /// <remarks>
    ///     <para>Options include DTD processing, whitespace handling, and conformance level.</para>
    ///     <para>Default is a new <see cref="XmlReaderSettings" /> instance.</para>
    /// </remarks>
    public XmlReaderSettings XmlReaderSettings { get; set; } = new();

    /// <summary>Gets or sets the XML writer settings used when serializing Lambda event payloads.</summary>
    /// <remarks>
    ///     <para>Options include indentation, encoding, and XML declaration behavior.</para>
    ///     <para>Default is a new <see cref="XmlWriterSettings" /> instance.</para>
    /// </remarks>
    public XmlWriterSettings XmlWriterSettings { get; set; } = new();
}
