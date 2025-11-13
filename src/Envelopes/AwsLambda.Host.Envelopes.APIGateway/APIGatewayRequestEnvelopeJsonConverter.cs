using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;

namespace AwsLambda.Host.Envelopes.APIGateway;

/// <summary>JSON converter for AWS API Gateway request envelopes with typed body payloads.</summary>
/// <typeparam name="T">The type of the deserialized body payload.</typeparam>
/// <remarks>
///     Handles serialization and deserialization of <see cref="APIGatewayRequestEnvelope{T}" />
///     instances, converting the body string to and from the specified type <typeparamref name="T" />.
/// </remarks>
public class APIGatewayRequestEnvelopeJsonConverter<T>
    : EnvelopeJsonConverter<APIGatewayRequestEnvelope<T>>
{
    /// <inheritdoc />
    protected override void ReadPayload(
        APIGatewayRequestEnvelope<T> value,
        JsonSerializerOptions options
    ) => value.Body = JsonSerializer.Deserialize<T>(((APIGatewayProxyRequest)value).Body, options);

    /// <inheritdoc />
    protected override void WritePayload(
        APIGatewayRequestEnvelope<T> value,
        JsonSerializerOptions options
    ) => ((APIGatewayProxyRequest)value).Body = JsonSerializer.Serialize(value.Body, options);

    /// <inheritdoc />
    protected override JsonConverter GetConverterInstance() => this;
}
