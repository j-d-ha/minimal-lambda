using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using AwsLambda.Host.Options;

namespace AwsLambda.Host.Envelopes.ApiGateway2;

/// <inheritdoc cref="Amazon.Lambda.APIGatewayEvents.APIGatewayProxyResponse" />
/// <remarks>
///     This class extends <see cref="Amazon.Lambda.APIGatewayEvents.APIGatewayProxyResponse" />
///     and adds a strongly typed <see cref="BodyContent" /> property for easier serialization and
///     deserialization of response payloads.
/// </remarks>
public class ApiGatewayResponseEnvelope<T> : APIGatewayProxyResponse, IResponseEnvelope
{
    /// <summary>The unserialized content of the <see cref="APIGatewayProxyResponse.Body" /></summary>
    [JsonIgnore]
    public T? BodyContent { get; set; }

    /// <inheritdoc />
    public void PackPayload(EnvelopeOptions options) =>
        Body = JsonSerializer.Serialize(BodyContent, options.JsonOptions);
}
