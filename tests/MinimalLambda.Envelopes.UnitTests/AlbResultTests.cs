using System.Text.Json;
using Amazon.Lambda.ApplicationLoadBalancerEvents;
using AutoFixture;
using AwesomeAssertions;
using JetBrains.Annotations;
using MinimalLambda.Envelopes.Alb;
using MinimalLambda.Options;
using Xunit;

namespace MinimalLambda.Envelopes.UnitTests;

[TestSubject(typeof(AlbResult))]
public class AlbResultTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void Create_WithBodyContent_SetsPropertiesCorrectly()
    {
        // Arrange
        var statusCode = 200;
        var payload = _fixture.Create<TestPayload>();
        var headers = new Dictionary<string, string> { ["X-Custom"] = "Value" };
        var isBase64Encoded = true;

        // Act
        var result = AlbResult.Create(statusCode, payload, headers, isBase64Encoded);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(statusCode);
        result.Headers.Should().ContainKey("X-Custom");
        result.Headers["X-Custom"].Should().Be("Value");
        result.IsBase64Encoded.Should().Be(isBase64Encoded);
        result.Body.Should().BeNull();
    }

    [Fact]
    public void PackPayload_SerializesBodyContent()
    {
        // Arrange
        var payload = _fixture.Create<TestPayload>();
        var options = new EnvelopeOptions();
        var result = AlbResult.Create(200, payload, new Dictionary<string, string>(), false);

        // Act
        result.PackPayload(options);

        // Assert
        result.Body.Should().NotBeNull();
        var deserialized = JsonSerializer.Deserialize<TestPayload>(result.Body);
        deserialized.Should().NotBeNull();
        deserialized.Name.Should().Be(payload.Name);
        deserialized.Value.Should().Be(payload.Value);
    }

    [Fact]
    public void AlbResult_InheritsFromApplicationLoadBalancerResponse()
    {
        // Arrange
        var result = AlbResult.Create(200, "test", new Dictionary<string, string>(), false);

        // Act & Assert
        result.Should().BeAssignableTo<ApplicationLoadBalancerResponse>();
    }

    [Fact]
    public void Create_WithNullBodyContent_HandlesNull()
    {
        // Arrange
        var options = new EnvelopeOptions();

        // Act
        var result = AlbResult.Create<object?>(204, null, new Dictionary<string, string>(), false);
        result.PackPayload(options);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(204);
        result.Body.Should().Be("null");
    }

    [Fact]
    public void Create_WithEnvelope_CreatesResultCorrectly()
    {
        // Arrange
        var payload = _fixture.Create<TestPayload>();
        var envelope = new AlbResponseEnvelope<TestPayload>
        {
            StatusCode = 201,
            BodyContent = payload,
            Headers = new Dictionary<string, string> { ["X-Test"] = "Value" },
            IsBase64Encoded = false,
        };

        // Act
        var result = AlbResult.Create(envelope);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(201);
        result.Headers.Should().ContainKey("X-Test");
        result.Headers["X-Test"].Should().Be("Value");
        result.IsBase64Encoded.Should().Be(false);
    }

    private record TestPayload(string Name, int Value);
}
