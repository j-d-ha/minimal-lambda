using AwsLambda.Host.Options;

namespace AwsLambda.Host;

public interface IEnvelope
{
    void ExtractPayload(EnvelopeOptions options);

    void PackPayload(EnvelopeOptions options);
}
