using System.Text.Json;
using System.Text.Json.Serialization;

namespace AwsLambda.Host.Envelopes;

/// <summary>Abstract base class for JSON converters that handle AWS Lambda event envelopes.</summary>
/// <typeparam name="TEnvelope">The type of the Lambda event to convert.</typeparam>
/// <remarks>
///     This converter manages serialization and deserialization of Lambda event envelopes with
///     custom payload handling. It maintains a separate <see cref="JsonSerializerOptions" /> instance
///     to avoid conflicts with user-configured naming policies.
/// </remarks>
public abstract class EnvelopeJsonConverter<TEnvelope> : JsonConverter<TEnvelope>
{
    private JsonSerializerOptions? _eventOptions;

    /// <summary>Deserializes custom payload data from the Lambda event after the base event has been read.</summary>
    /// <param name="value">The deserialized Lambda event instance.</param>
    /// <param name="options">The JSON serializer options provided by the user.</param>
    protected abstract void ReadPayload(TEnvelope value, JsonSerializerOptions options);

    /// <summary>Serializes custom payload data into the Lambda event before serialization.</summary>
    /// <param name="value">The Lambda event instance to serialize.</param>
    /// <param name="options">The JSON serializer options provided by the user.</param>
    protected abstract void WritePayload(TEnvelope value, JsonSerializerOptions options);

    /// <summary>Returns the converter instance to be removed from the JSON serializer options.</summary>
    /// <returns>The converter instance used by this envelope.</returns>
    /// <remarks>This method is AOT-safe as it returns an instance reference known at compile time.</remarks>
    protected abstract JsonConverter GetConverterInstance();

    /// <summary>Reads and deserializes a Lambda event envelope from JSON.</summary>
    /// <param name="reader">The JSON reader to deserialize from.</param>
    /// <param name="typeToConvert">The type to convert to.</param>
    /// <param name="options">The JSON serializer options provided by the user.</param>
    /// <returns>The deserialized Lambda event, or null if the input is null.</returns>
    public override TEnvelope? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        SetJsonSerializerOptions(options);

        var baseEvent = JsonSerializer.Deserialize<TEnvelope>(ref reader, _eventOptions);

        if (baseEvent is null)
            return default;

        ReadPayload(baseEvent, options);

        return baseEvent;
    }

    /// <summary>Writes and serializes a Lambda event envelope to JSON.</summary>
    /// <param name="writer">The JSON writer to serialize to.</param>
    /// <param name="value">The Lambda event instance to serialize.</param>
    /// <param name="options">The JSON serializer options provided by the user.</param>
    public override void Write(
        Utf8JsonWriter writer,
        TEnvelope value,
        JsonSerializerOptions options
    )
    {
        SetJsonSerializerOptions(options);

        WritePayload(value, options);

        JsonSerializer.Serialize(writer, value, _eventOptions);
    }

    /// <summary>
    ///     Use a separate JsonSerializerOptions for the APIGatewayProxyRequest to avoid any conflicts
    ///     with the user's options' for naming policies.
    /// </summary>
    /// <param name="options"></param>
    private void SetJsonSerializerOptions(JsonSerializerOptions options)
    {
        if (_eventOptions is not null)
            return;

        _eventOptions = new JsonSerializerOptions(options) { PropertyNamingPolicy = null };
        _eventOptions.Converters.Remove(GetConverterInstance());
    }
}
