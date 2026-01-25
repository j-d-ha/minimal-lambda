using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using AutoFixture;
using AwesomeAssertions;
using JetBrains.Annotations;
using MinimalLambda.Envelopes.ApiGateway;
using MinimalLambda.Options;
using Xunit;

namespace MinimalLambda.Envelopes.UnitTests;

[TestSubject(typeof(ApiGatewayV2Result))]
public class ApiGatewayV2ResultTests
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
        var result = ApiGatewayV2Result.Create(statusCode, payload, headers, isBase64Encoded);

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
        var result = ApiGatewayV2Result.Create(
            200,
            payload,
            new Dictionary<string, string>(),
            false);

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
    public void ApiGatewayV2Result_InheritsFromAPIGatewayHttpApiV2ProxyResponse()
    {
        // Arrange
        var result = ApiGatewayV2Result.Create(
            200,
            "test",
            new Dictionary<string, string>(),
            false);

        // Act & Assert
        result.Should().BeAssignableTo<APIGatewayHttpApiV2ProxyResponse>();
    }

    [Fact]
    public void Create_WithNullBodyContent_HandlesNull()
    {
        // Arrange
        var options = new EnvelopeOptions();

        // Act
        var result = ApiGatewayV2Result.Create<object?>(
            204,
            null,
            new Dictionary<string, string>(),
            false);
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
        var envelope = new ApiGatewayV2ResponseEnvelope<TestPayload>
        {
            StatusCode = 201,
            BodyContent = payload,
            Headers = new Dictionary<string, string> { ["X-Test"] = "Value" },
            IsBase64Encoded = false,
        };

        // Act
        var result = ApiGatewayV2Result.Create(envelope);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(201);
        result.Headers.Should().ContainKey("X-Test");
        result.Headers["X-Test"].Should().Be("Value");
        result.IsBase64Encoded.Should().Be(false);
    }

    private record TestPayload(string Name, int Value);
}
