using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;

namespace AwsLambda.Host.Envelopes.APIGateway;

/// <summary>JSON converter for AWS API Gateway response envelopes with typed body payloads.</summary>
/// <typeparam name="T">The type of the deserialized body payload.</typeparam>
/// <remarks>
///     Handles serialization and deserialization of <see cref="APIGatewayResponseEnvelope{T}" />
///     instances, converting the body string to and from the specified type <typeparamref name="T" />.
/// </remarks>
public class APIGatewayResponseJsonConverter<T>
    : EnvelopeJsonConverter<APIGatewayResponseEnvelope<T>>
{
    /// <inheritdoc />
    protected override void ReadPayload(
        APIGatewayResponseEnvelope<T> value,
        JsonSerializerOptions options
    ) => value.Body = JsonSerializer.Deserialize<T>(((APIGatewayProxyResponse)value).Body, options);

    /// <inheritdoc />
    protected override void WritePayload(
        APIGatewayResponseEnvelope<T> value,
        JsonSerializerOptions options
    ) => ((APIGatewayProxyResponse)value).Body = JsonSerializer.Serialize(value.Body, options);

    /// <inheritdoc />
    protected override JsonConverter GetConverterInstance() => this;
}
