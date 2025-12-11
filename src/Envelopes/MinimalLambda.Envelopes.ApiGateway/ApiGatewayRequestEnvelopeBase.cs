using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using AwsLambda.Host.Envelopes;
using AwsLambda.Host.Options;

namespace MinimalLambda.Envelopes.ApiGateway;

/// <inheritdoc cref="Amazon.Lambda.APIGatewayEvents.APIGatewayProxyRequest" />
/// <remarks>
///     This abstract class extends
///     <see cref="Amazon.Lambda.APIGatewayEvents.APIGatewayProxyRequest" /> and provides a foundation
///     for strongly typed request handling. Derived classes implement <see cref="ExtractPayload" /> to
///     deserialize the request body into a strongly typed <see cref="BodyContent" /> property using
///     their chosen deserialization strategy.
/// </remarks>
public abstract class ApiGatewayRequestEnvelopeBase<T> : APIGatewayProxyRequest, IRequestEnvelope
{
    /// <summary>The deserialized content of the <see cref="APIGatewayProxyRequest.Body" /></summary>
    [JsonIgnore]
    public T? BodyContent { get; set; }

    /// <inheritdoc />
    public abstract void ExtractPayload(EnvelopeOptions options);
}
