using AwsLambda.Host.Envelopes.ApiGateway;
using JetBrains.Annotations;
using Xunit;

namespace AwsLambda.Host.Envelopes.UnitTests;

[TestSubject(typeof(ApiGatewayRequestEnvelope<>))]
public class ApiGatewayRequestEnvelopeTests
{
    [Fact]
    public void Test1() { }
}
