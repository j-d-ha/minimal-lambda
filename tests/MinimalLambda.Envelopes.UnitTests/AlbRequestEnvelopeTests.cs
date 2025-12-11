using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.ApplicationLoadBalancerEvents;
using AutoFixture;
using AwesomeAssertions;
using AwsLambda.Host.Options;
using JetBrains.Annotations;
using MinimalLambda.Envelopes.Alb;
using Xunit;

namespace MinimalLambda.Envelopes.UnitTests;

[TestSubject(typeof(AlbRequestEnvelope<>))]
public class AlbRequestEnvelopeTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void ExtractPayload_WithValidJson_PopulatesBodyContent()
    {
        // Arrange
        var testData = _fixture.Create<TestPayload>();
        var json = JsonSerializer.Serialize(testData);
        var envelope = new AlbRequestEnvelope<TestPayload> { Body = json };
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
        var envelope = new AlbRequestEnvelope<TestPayload> { Body = json };
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
    public void ExtractPayload_WithValidNullValue_SetsBodyContentToNull()
    {
        // Arrange
        var envelope = new AlbRequestEnvelope<TestPayload> { Body = "null" };
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
        var property = typeof(AlbRequestEnvelope<TestPayload>).GetProperty(
            nameof(AlbRequestEnvelope<TestPayload>.BodyContent)
        );

        // Act
        var hasJsonIgnoreAttribute =
            property?.GetCustomAttributes(typeof(JsonIgnoreAttribute), false).Length > 0;

        // Assert
        hasJsonIgnoreAttribute.Should().BeTrue();
    }

    [Fact]
    public void AlbRequestEnvelope_InheritsFromApplicationLoadBalancerRequest()
    {
        // Arrange & Act
        var envelope = new AlbRequestEnvelope<TestPayload>();

        // Assert
        envelope.Should().BeAssignableTo<ApplicationLoadBalancerRequest>();
    }

    [Fact]
    public void ExtractPayload_WithComplexObject_PreservesAllProperties()
    {
        // Arrange
        var testData = _fixture.Create<TestPayload>();
        var json = JsonSerializer.Serialize(testData);
        var envelope = new AlbRequestEnvelope<TestPayload> { Body = json };
        var options = new EnvelopeOptions();

        // Act
        envelope.ExtractPayload(options);

        // Assert
        envelope.BodyContent.Should().NotBeNull();
        envelope.BodyContent!.Name.Should().Be(testData.Name);
        envelope.BodyContent.Value.Should().Be(testData.Value);
    }

    private record TestPayload(string Name, int Value);
}
