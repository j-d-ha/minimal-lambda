using System.Text.Json;
using AwsLambda.Host.Envelopes;

namespace AwsLambda.Host.Options;

/// <summary>Options for configuring envelope payload serialization and deserialization.</summary>
public class EnvelopeOptions
{
    /// <summary>
    ///     Gets or sets the JSON serialization options used when extracting and packing Lambda event
    ///     payloads.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         These options are passed to implementations of <see cref="IRequestEnvelope" /> and
    ///         <see cref="IResponseEnvelope" /> to control how payloads are serialized and deserialized.
    ///         Custom converters, naming policies, and other JSON serialization settings can be configured
    ///         here.
    ///     </para>
    ///     <para>Default is an empty <see cref="JsonSerializerOptions" /> instance.</para>
    /// </remarks>
    /// <seealso cref="IRequestEnvelope" />
    /// <seealso cref="IResponseEnvelope" />
    public JsonSerializerOptions JsonOptions { get; set; } = new();
}
