using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using AwsLambda.Host.Envelopes;
using AwsLambda.Host.Options;

namespace MinimalLambda.Envelopes.ApiGateway;

/// <inheritdoc cref="Amazon.Lambda.APIGatewayEvents.APIGatewayProxyResponse" />
/// <remarks>
///     This abstract class extends
///     <see cref="Amazon.Lambda.APIGatewayEvents.APIGatewayProxyResponse" /> and provides a foundation
///     for strongly typed response handling. Derived classes implement <see cref="PackPayload" /> to
///     serialize the strongly typed <see cref="BodyContent" /> property into the response body using
///     their chosen serialization strategy.
/// </remarks>
public abstract class ApiGatewayResponseEnvelopeBase<T> : APIGatewayProxyResponse, IResponseEnvelope
{
    /// <summary>The deserialized content of the <see cref="APIGatewayProxyResponse.Body" /></summary>
    [JsonIgnore]
    public T? BodyContent { get; set; }

    /// <inheritdoc />
    public abstract void PackPayload(EnvelopeOptions options);
}
