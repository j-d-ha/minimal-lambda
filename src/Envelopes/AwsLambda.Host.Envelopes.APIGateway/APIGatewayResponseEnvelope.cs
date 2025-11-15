using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using AwsLambda.Host.Options;

namespace AwsLambda.Host.Envelopes.APIGateway;

/// <inheritdoc cref="Amazon.Lambda.APIGatewayEvents.APIGatewayProxyResponse" />
public class APIGatewayResponseEnvelope<T> : APIGatewayProxyResponse, IEnvelope
{
    /// <summary>The content of the response body</summary>
    [JsonIgnore]
    public T? BodyContent { get; set; }

    public void ExtractPayload(EnvelopeOptions options) =>
        BodyContent = JsonSerializer.Deserialize<T>(Body, options.JsonOptions);

    public void PackPayload(EnvelopeOptions options) =>
        Body = JsonSerializer.Serialize(BodyContent, options.JsonOptions);
}
