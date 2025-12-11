using System.Text.Json.Serialization;
using Amazon.Lambda.ApplicationLoadBalancerEvents;
using MinimalLambda.Options;

namespace MinimalLambda.Envelopes.Alb;

/// <inheritdoc cref="Amazon.Lambda.ApplicationLoadBalancerEvents.ApplicationLoadBalancerRequest" />
/// <remarks>
///     This abstract class extends
///     <see cref="Amazon.Lambda.ApplicationLoadBalancerEvents.ApplicationLoadBalancerRequest" /> and
///     provides a foundation for strongly typed request handling. Derived classes implement
///     <see cref="ExtractPayload" /> to deserialize the request body into a strongly typed
///     <see cref="BodyContent" /> property using their chosen deserialization strategy.
/// </remarks>
public abstract class AlbRequestEnvelopeBase<T> : ApplicationLoadBalancerRequest, IRequestEnvelope
{
    /// <summary>The deserialized content of the <see cref="ApplicationLoadBalancerRequest.Body" /></summary>
    [JsonIgnore]
    public T? BodyContent { get; set; }

    /// <inheritdoc />
    public abstract void ExtractPayload(EnvelopeOptions options);
}
