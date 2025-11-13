using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;

namespace AwsLambda.Host.Envelopes.APIGateway;

/// <inheritdoc cref="Amazon.Lambda.APIGatewayEvents.APIGatewayProxyRequest" />
public class APIGatewayRequestEnvelope<T> : APIGatewayProxyRequest, IJsonSerializable
{
    /// <summary>The deserialized content of the HTTP request body.</summary>
    [JsonIgnore]
    public new T? Body { get; set; }

    /// <inheritdoc />
    public static void RegisterConverter(IList<JsonConverter> converters) =>
        converters.Add(new APIGatewayRequestEnvelopeJsonConverter<T>());
}
