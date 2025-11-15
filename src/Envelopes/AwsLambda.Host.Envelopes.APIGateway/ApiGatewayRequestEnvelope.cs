using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using AwsLambda.Host.Options;

namespace AwsLambda.Host.Envelopes.APIGateway;

/// <inheritdoc cref="Amazon.Lambda.APIGatewayEvents.APIGatewayProxyRequest" />
/// <remarks>
///     This class extends <see cref="Amazon.Lambda.APIGatewayEvents.APIGatewayProxyRequest" />
///     and adds a strongly typed <see cref="BodyContent" /> property for easier serialization and
///     deserialization of request payloads.
/// </remarks>
public class ApiGatewayRequestEnvelope<T> : APIGatewayProxyRequest, IEnvelope
{
    /// <summary>The deserialized content of the <see cref="APIGatewayProxyRequest.Body" /></summary>
    [JsonIgnore]
    public T? BodyContent { get; set; }

    public void ExtractPayload(EnvelopeOptions options) =>
        BodyContent = JsonSerializer.Deserialize<T>(Body, options.JsonOptions);

    public void PackPayload(EnvelopeOptions options) =>
        Body = JsonSerializer.Serialize(BodyContent, options.JsonOptions);
}
