using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.ApplicationLoadBalancerEvents;
using AutoFixture;
using AwesomeAssertions;
using AwsLambda.Host.Envelopes.Alb;
using AwsLambda.Host.Options;
using JetBrains.Annotations;
using Xunit;

namespace AwsLambda.Host.Envelopes.UnitTests;

[TestSubject(typeof(AlbResponseEnvelope<>))]
public class AlbResponseEnvelopeTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void PackPayload_WithValidData_SerializesBodyContent()
    {
        // Arrange
        var responseData = _fixture.Create<ResponsePayload>();
        var envelope = new AlbResponseEnvelope<ResponsePayload> { BodyContent = responseData };
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
        var envelope = new AlbResponseEnvelope<ResponsePayload> { BodyContent = responseData };
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
        var envelope = new AlbResponseEnvelope<ResponsePayload> { BodyContent = null };
        var options = new EnvelopeOptions();

        // Act
        envelope.PackPayload(options);

        // Assert
        envelope.Body.Should().NotBeNull();
        envelope.Body.Should().Be("null");
    }

    [Fact]
    public void BodyContent_HasJsonIgnoreAttribute()
    {
        // Arrange
        var property = typeof(AlbResponseEnvelope<ResponsePayload>).GetProperty(
            nameof(AlbResponseEnvelope<ResponsePayload>.BodyContent)
        );

        // Act
        var hasJsonIgnoreAttribute =
            property?.GetCustomAttributes(typeof(JsonIgnoreAttribute), false).Length > 0;

        // Assert
        hasJsonIgnoreAttribute.Should().BeTrue();
    }

    [Fact]
    public void AlbResponseEnvelope_InheritsFromApplicationLoadBalancerResponse()
    {
        // Arrange & Act
        var envelope = new AlbResponseEnvelope<ResponsePayload>();

        // Assert
        envelope.Should().BeAssignableTo<ApplicationLoadBalancerResponse>();
    }

    [Fact]
    public void PackPayload_WithComplexObject_PreservesAllProperties()
    {
        // Arrange
        var responseData = _fixture.Create<ResponsePayload>();
        var envelope = new AlbResponseEnvelope<ResponsePayload> { BodyContent = responseData };
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

    private record ResponsePayload(string Message, int Code);
}
