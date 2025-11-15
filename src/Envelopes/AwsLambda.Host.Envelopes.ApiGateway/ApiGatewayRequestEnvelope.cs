using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using AwsLambda.Host.Options;

namespace AwsLambda.Host.Envelopes.ApiGateway2;

/// <inheritdoc cref="Amazon.Lambda.APIGatewayEvents.APIGatewayProxyRequest" />
/// <remarks>
///     This class extends <see cref="Amazon.Lambda.APIGatewayEvents.APIGatewayProxyRequest" />
///     and adds a strongly typed <see cref="BodyContent" /> property for easier serialization and
///     deserialization of request payloads.
/// </remarks>
public class ApiGatewayRequestEnvelope<T> : APIGatewayProxyRequest, IRequestEnvelope
{
    /// <summary>The deserialized content of the <see cref="APIGatewayProxyRequest.Body" /></summary>
    [JsonIgnore]
    public T? BodyContent { get; set; }

    /// <inheritdoc />
    public void ExtractPayload(EnvelopeOptions options) =>
        BodyContent = JsonSerializer.Deserialize<T>(Body, options.JsonOptions);
}
