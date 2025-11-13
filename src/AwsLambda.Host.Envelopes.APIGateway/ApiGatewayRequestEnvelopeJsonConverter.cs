using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;

namespace AwsLambda.Host.APIGatewayEnvelops;

/// <summary>JSON converter for AWS API Gateway request envelopes with typed body payloads.</summary>
/// <typeparam name="T">The type of the deserialized body payload.</typeparam>
/// <remarks>
///     Handles serialization and deserialization of <see cref="ApiGatewayRequestEnvelope{T}" />
///     instances, converting the body string to and from the specified type <typeparamref name="T" />.
/// </remarks>
public class ApiGatewayRequestEnvelopeJsonConverter<T>
    : EnvelopeJsonConverter<ApiGatewayRequestEnvelope<T>>
{
    /// <inheritdoc />
    protected override void ReadPayload(
        ApiGatewayRequestEnvelope<T> value,
        JsonSerializerOptions options
    ) => value.Body = JsonSerializer.Deserialize<T>(((APIGatewayProxyRequest)value).Body, options);

    /// <inheritdoc />
    protected override void WritePayload(
        ApiGatewayRequestEnvelope<T> value,
        JsonSerializerOptions options
    ) => ((APIGatewayProxyRequest)value).Body = JsonSerializer.Serialize(value.Body, options);

    /// <inheritdoc />
    protected override JsonConverter GetConverterInstance() => this;
}
