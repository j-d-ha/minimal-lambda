using System.Text.Json.Serialization;
using Amazon.Lambda.ApplicationLoadBalancerEvents;
using AwsLambda.Host.Envelopes;
using AwsLambda.Host.Options;

namespace MinimalLambda.Envelopes.Alb;

/// <inheritdoc cref="Amazon.Lambda.ApplicationLoadBalancerEvents.ApplicationLoadBalancerResponse" />
/// <remarks>
///     This abstract class extends
///     <see cref="Amazon.Lambda.ApplicationLoadBalancerEvents.ApplicationLoadBalancerResponse" /> and
///     provides a foundation for strongly typed response handling. Derived classes implement
///     <see cref="PackPayload" /> to serialize the strongly typed <see cref="BodyContent" /> property
///     into the response body using their chosen serialization strategy.
/// </remarks>
public abstract class AlbResponseEnvelopeBase<T>
    : ApplicationLoadBalancerResponse,
        IResponseEnvelope
{
    /// <summary>The deserialized content of the <see cref="ApplicationLoadBalancerResponse.Body" /></summary>
    [JsonIgnore]
    public T? BodyContent { get; set; }

    /// <inheritdoc />
    public abstract void PackPayload(EnvelopeOptions options);
}
