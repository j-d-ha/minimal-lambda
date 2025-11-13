using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;

namespace AwsLambda.Host.Envelopes.APIGateway;

/// <inheritdoc cref="Amazon.Lambda.APIGatewayEvents.APIGatewayProxyResponse" />
public class APIGatewayResponseEnvelope<T> : APIGatewayProxyResponse, IJsonSerializable
{
    /// <summary>The content of the response body</summary>
    [JsonIgnore]
    public new T? Body { get; set; }

    /// <inheritdoc />
    public static void RegisterConverter(IList<JsonConverter> converters) =>
        converters.Add(new APIGatewayResponseJsonConverter<T>());
}
