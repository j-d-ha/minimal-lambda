using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using AwsLambda.Host.Options;

namespace AwsLambda.Host.Envelopes.APIGateway;

/// <inheritdoc cref="Amazon.Lambda.APIGatewayEvents.APIGatewayProxyRequest" />
public class APIGatewayRequestEnvelope<T> : APIGatewayProxyRequest, IEnvelope
{
    /// <summary>The deserialized content of the HTTP request body.</summary>
    [JsonIgnore]
    public T? BodyContent { get; set; }

    public void ExtractPayload(EnvelopeOptions options) =>
        BodyContent = JsonSerializer.Deserialize<T>(Body, options.JsonOptions);

    public void PackPayload(EnvelopeOptions options) =>
        Body = JsonSerializer.Serialize(BodyContent, options.JsonOptions);
}
