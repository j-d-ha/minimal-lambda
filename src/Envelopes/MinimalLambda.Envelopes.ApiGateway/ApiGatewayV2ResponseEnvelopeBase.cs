using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using AwsLambda.Host.Envelopes;
using AwsLambda.Host.Options;

namespace MinimalLambda.Envelopes.ApiGateway;

/// <inheritdoc cref="Amazon.Lambda.APIGatewayEvents.APIGatewayHttpApiV2ProxyResponse" />
/// <remarks>
///     This abstract class extends
///     <see cref="Amazon.Lambda.APIGatewayEvents.APIGatewayHttpApiV2ProxyResponse" /> and provides a
///     foundation for strongly typed response handling. Derived classes implement
///     <see cref="PackPayload" /> to serialize the strongly typed <see cref="BodyContent" /> property
///     into the response body using their chosen serialization strategy.
/// </remarks>
public abstract class ApiGatewayV2ResponseEnvelopeBase<T>
    : APIGatewayHttpApiV2ProxyResponse,
        IResponseEnvelope
{
    /// <summary>The deserialized content of the <see cref="APIGatewayHttpApiV2ProxyResponse.Body" /></summary>
    [JsonIgnore]
    public T? BodyContent { get; set; }

    /// <inheritdoc />
    public abstract void PackPayload(EnvelopeOptions options);
}
