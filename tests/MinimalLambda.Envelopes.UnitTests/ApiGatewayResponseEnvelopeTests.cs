using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using AutoFixture;
using AwesomeAssertions;
using JetBrains.Annotations;
using MinimalLambda.Envelopes.ApiGateway;
using MinimalLambda.Options;
using Xunit;

namespace MinimalLambda.Envelopes.UnitTests;

[TestSubject(typeof(ApiGatewayResponseEnvelope<>))]
public class ApiGatewayResponseEnvelopeTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void PackPayload_WithValidData_SerializesBodyContent()
    {
        // Arrange
        var responseData = _fixture.Create<ResponsePayload>();
        var envelope = new ApiGatewayResponseEnvelope<ResponsePayload>
        {
            BodyContent = responseData,
        };
        var options = new EnvelopeOptions();

        // Act
        envelope.PackPayload(options);

        // Assert
        envelope.Body.Should().NotBeNull();
        var deserializedData = JsonSerializer.Deserialize<ResponsePayload>(
            envelope.Body,
            options.JsonOptions
        );
        deserializedData.Should().NotBeNull();
        deserializedData!.Message.Should().Be(responseData.Message);
        deserializedData.Code.Should().Be(responseData.Code);
    }

    [Fact]
    public void PackPayload_WithCamelCaseNamingPolicy_SerializesWithCamelCaseKeys()
    {
        // Arrange
        var responseData = new ResponsePayload("test", 42);
        var envelope = new ApiGatewayResponseEnvelope<ResponsePayload>
        {
            BodyContent = responseData,
        };
        var options = new EnvelopeOptions
        {
            JsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            },
        };

        // Act
        envelope.PackPayload(options);

        // Assert
        envelope.Body.Should().NotBeNull();
        envelope.Body.Should().Contain("\"message\"");
        envelope.Body.Should().Contain("\"code\"");
        envelope.Body.Should().NotContain("\"Message\"");
        envelope.Body.Should().NotContain("\"Code\"");
    }

    [Fact]
    public void PackPayload_WithNullBodyContent_SerializesNull()
    {
        // Arrange
        var envelope = new ApiGatewayResponseEnvelope<ResponsePayload> { BodyContent = null };
        var options = new EnvelopeOptions();

        // Act
        envelope.PackPayload(options);

        // Assert
        envelope.Body.Should().NotBeNull();
        envelope.Body.Should().Be("null");
    }

    [Fact]
    public void PackPayload_WithComplexObject_PreservesAllProperties()
    {
        // Arrange
        var responseData = _fixture.Create<ResponsePayload>();
        var envelope = new ApiGatewayResponseEnvelope<ResponsePayload>
        {
            BodyContent = responseData,
        };
        var options = new EnvelopeOptions();

        // Act
        envelope.PackPayload(options);

        // Assert
        envelope.Body.Should().NotBeNull();
        var deserializedData = JsonSerializer.Deserialize<ResponsePayload>(envelope.Body);
        deserializedData.Should().NotBeNull();
        deserializedData!.Message.Should().Be(responseData.Message);
        deserializedData.Code.Should().Be(responseData.Code);
    }

    [Fact]
    public void BodyContent_HasJsonIgnoreAttribute()
    {
        // Arrange
        var property = typeof(ApiGatewayResponseEnvelope<ResponsePayload>).GetProperty(
            nameof(ApiGatewayResponseEnvelope<ResponsePayload>.BodyContent)
        );

        // Act
        var hasJsonIgnoreAttribute =
            property?.GetCustomAttributes(typeof(JsonIgnoreAttribute), false).Length > 0;

        // Assert
        hasJsonIgnoreAttribute.Should().BeTrue();
    }

    [Fact]
    public void ApiGatewayResponseEnvelope_InheritsFromApiGatewayProxyResponse()
    {
        // Arrange & Act
        var envelope = new ApiGatewayResponseEnvelope<ResponsePayload>();

        // Assert
        envelope.Should().BeAssignableTo<APIGatewayProxyResponse>();
    }

    [Fact]
    public void PackPayload_WithMultipleInvocations_UpdatesBodyEachTime()
    {
        // Arrange
        var firstData = new ResponsePayload("First", 1);
        var secondData = new ResponsePayload("Second", 2);
        var envelope = new ApiGatewayResponseEnvelope<ResponsePayload> { BodyContent = firstData };
        var options = new EnvelopeOptions();

        // Act
        envelope.PackPayload(options);
        var firstBody = envelope.Body;

        envelope.BodyContent = secondData;
        envelope.PackPayload(options);
        var secondBody = envelope.Body;

        // Assert
        firstBody.Should().NotBeNull();
        secondBody.Should().NotBeNull();
        firstBody.Should().NotBe(secondBody);
        var firstDeserialized = JsonSerializer.Deserialize<ResponsePayload>(firstBody);
        var secondDeserialized = JsonSerializer.Deserialize<ResponsePayload>(secondBody);
        firstDeserialized!.Message.Should().Be("First");
        secondDeserialized!.Message.Should().Be("Second");
    }

    [Fact]
    public void PackPayload_WithEmptyString_SerializesEmptyString()
    {
        // Arrange
        var envelope = new ApiGatewayResponseEnvelope<string> { BodyContent = string.Empty };
        var options = new EnvelopeOptions();

        // Act
        envelope.PackPayload(options);

        // Assert
        envelope.Body.Should().NotBeNull();
        envelope.Body.Should().Be("\"\"");
    }

    [Fact]
    public void PackPayload_WithCustomConverter_UsesCustomSerialization()
    {
        // Arrange
        var responseData = _fixture.Create<ResponsePayload>();
        var envelope = new ApiGatewayResponseEnvelope<ResponsePayload>
        {
            BodyContent = responseData,
        };
        var options = new EnvelopeOptions
        {
            JsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
            },
        };

        // Act
        envelope.PackPayload(options);

        // Assert
        envelope.Body.Should().NotBeNull();
        envelope.Body.Should().Contain("\n"); // WriteIndented adds newlines
        var deserializedData = JsonSerializer.Deserialize<ResponsePayload>(
            envelope.Body,
            options.JsonOptions
        );
        deserializedData.Should().NotBeNull();
        deserializedData!.Message.Should().Be(responseData.Message);
    }

    private record ResponsePayload(string Message, int Code);
}
