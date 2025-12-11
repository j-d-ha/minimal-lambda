using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using AutoFixture;
using AwesomeAssertions;
using AwsLambda.Host.Options;
using JetBrains.Annotations;
using MinimalLambda.Envelopes.ApiGateway;
using Xunit;

namespace MinimalLambda.Envelopes.UnitTests;

[TestSubject(typeof(ApiGatewayRequestEnvelope<>))]
public class ApiGatewayRequestEnvelopeTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void ExtractPayload_WithValidJson_PopulatesBodyContent()
    {
        // Arrange
        var testData = _fixture.Create<TestPayload>();
        var json = JsonSerializer.Serialize(testData);
        var envelope = new ApiGatewayRequestEnvelope<TestPayload> { Body = json };
        var options = new EnvelopeOptions();

        // Act
        envelope.ExtractPayload(options);

        // Assert
        envelope.BodyContent.Should().NotBeNull();
        envelope.BodyContent!.Name.Should().Be(testData.Name);
        envelope.BodyContent.Value.Should().Be(testData.Value);
    }

    [Fact]
    public void ExtractPayload_WithCamelCaseNamingPolicy_DeserializesCorrectly()
    {
        // Arrange
        var testData = _fixture.Create<TestPayload>();
        var json = JsonSerializer.Serialize(
            testData,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        );
        var envelope = new ApiGatewayRequestEnvelope<TestPayload> { Body = json };
        var options = new EnvelopeOptions
        {
            JsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            },
        };

        // Act
        envelope.ExtractPayload(options);

        // Assert
        envelope.BodyContent.Should().NotBeNull();
        envelope.BodyContent!.Name.Should().Be(testData.Name);
        envelope.BodyContent.Value.Should().Be(testData.Value);
    }

    [Fact]
    public void ExtractPayload_WithNullBody_ThrowsArgumentNullException()
    {
        // Arrange
        var envelope = new ApiGatewayRequestEnvelope<TestPayload> { Body = null };
        var options = new EnvelopeOptions();

        // Act & Assert
        var act = () => envelope.ExtractPayload(options);
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void ExtractPayload_WithEmptyBody_ThrowsJsonException()
    {
        // Arrange
        var envelope = new ApiGatewayRequestEnvelope<TestPayload> { Body = string.Empty };
        var options = new EnvelopeOptions();

        // Act & Assert
        var act = () => envelope.ExtractPayload(options);
        act.Should().ThrowExactly<JsonException>();
    }

    [Fact]
    public void ExtractPayload_WithInvalidJson_ThrowsJsonException()
    {
        // Arrange
        var invalidJson = _fixture.Create<string>(); // AutoFixture generates invalid JSON as raw string
        var envelope = new ApiGatewayRequestEnvelope<TestPayload> { Body = invalidJson };
        var options = new EnvelopeOptions();

        // Act & Assert
        var act = () => envelope.ExtractPayload(options);
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void ExtractPayload_WithValidNullValue_SetsBodyContentToNull()
    {
        // Arrange
        var envelope = new ApiGatewayRequestEnvelope<TestPayload> { Body = "null" };
        var options = new EnvelopeOptions();

        // Act
        envelope.ExtractPayload(options);

        // Assert
        envelope.BodyContent.Should().BeNull();
    }

    [Fact]
    public void BodyContent_HasJsonIgnoreAttribute()
    {
        // Arrange
        var property = typeof(ApiGatewayRequestEnvelope<TestPayload>).GetProperty(
            nameof(ApiGatewayRequestEnvelope<TestPayload>.BodyContent)
        );

        // Act
        var hasJsonIgnoreAttribute =
            property?.GetCustomAttributes(typeof(JsonIgnoreAttribute), false).Length > 0;

        // Assert
        hasJsonIgnoreAttribute.Should().BeTrue();
    }

    [Fact]
    public void ApiGatewayRequestEnvelope_InheritsFromApiGatewayProxyRequest()
    {
        // Arrange & Act
        var envelope = new ApiGatewayRequestEnvelope<TestPayload>();

        // Assert
        envelope.Should().BeAssignableTo<APIGatewayProxyRequest>();
    }

    [Fact]
    public void ExtractPayload_WithMultipleProperties_PreservesAllValues()
    {
        // Arrange
        var testData = _fixture.Create<TestPayload>();
        var json = JsonSerializer.Serialize(testData);
        var envelope = new ApiGatewayRequestEnvelope<TestPayload> { Body = json };
        var options = new EnvelopeOptions();

        // Act
        envelope.ExtractPayload(options);

        // Assert
        envelope.BodyContent.Should().NotBeNull();
        envelope.BodyContent!.Name.Should().Be(testData.Name);
        envelope.BodyContent.Value.Should().Be(testData.Value);
    }

    [Fact]
    public void ExtractPayload_WithMalformedJsonStructure_ThrowsJsonException()
    {
        // Arrange
        var malformedJson = """{"Name":"Valid","Value":"NotAnInt"}"""; // Value should be int, not string
        var envelope = new ApiGatewayRequestEnvelope<TestPayload> { Body = malformedJson };
        var options = new EnvelopeOptions();

        // Act & Assert
        var act = () => envelope.ExtractPayload(options);
        act.Should().Throw<JsonException>();
    }

    private record TestPayload(string Name, int Value);
}
