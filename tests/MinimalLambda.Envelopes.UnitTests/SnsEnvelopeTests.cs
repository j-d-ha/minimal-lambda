using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.SNSEvents;
using AutoFixture;
using AwesomeAssertions;
using JetBrains.Annotations;
using MinimalLambda.Envelopes.Sns;
using MinimalLambda.Options;
using Xunit;

namespace MinimalLambda.Envelopes.UnitTests;

[TestSubject(typeof(SnsEnvelope<>))]
public class SnsEnvelopeTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void ExtractPayload_WithSingleRecord_DeserializesBodyContent()
    {
        // Arrange
        var payload = _fixture.Create<MessagePayload>();
        var json = JsonSerializer.Serialize(payload);
        var record = new SnsEnvelopeBase<MessagePayload>.SnsRecordEnvelope
        {
            Sns = new SnsEnvelopeBase<MessagePayload>.SnsMessageEnvelope { Message = json },
        };
        var envelope = new SnsEnvelope<MessagePayload> { Records = [record] };
        var options = new EnvelopeOptions();

        // Act
        envelope.ExtractPayload(options);

        // Assert
        record.Sns.MessageContent.Should().NotBeNull();
        record.Sns.MessageContent!.Content.Should().Be(payload.Content);
        record.Sns.MessageContent.Priority.Should().Be(payload.Priority);
    }

    [Fact]
    public void ExtractPayload_WithMultipleRecords_DeserializesAllMessages()
    {
        // Arrange
        var payload1 = _fixture.Create<MessagePayload>();
        var payload2 = _fixture.Create<MessagePayload>();
        var payload3 = _fixture.Create<MessagePayload>();
        var json1 = JsonSerializer.Serialize(payload1);
        var json2 = JsonSerializer.Serialize(payload2);
        var json3 = JsonSerializer.Serialize(payload3);

        var record1 = new SnsEnvelopeBase<MessagePayload>.SnsRecordEnvelope
        {
            Sns = new SnsEnvelopeBase<MessagePayload>.SnsMessageEnvelope { Message = json1 },
        };
        var record2 = new SnsEnvelopeBase<MessagePayload>.SnsRecordEnvelope
        {
            Sns = new SnsEnvelopeBase<MessagePayload>.SnsMessageEnvelope { Message = json2 },
        };
        var record3 = new SnsEnvelopeBase<MessagePayload>.SnsRecordEnvelope
        {
            Sns = new SnsEnvelopeBase<MessagePayload>.SnsMessageEnvelope { Message = json3 },
        };

        var envelope = new SnsEnvelope<MessagePayload> { Records = [record1, record2, record3] };
        var options = new EnvelopeOptions();

        // Act
        envelope.ExtractPayload(options);

        // Assert
        record1.Sns.MessageContent.Should().NotBeNull();
        record1.Sns.MessageContent!.Content.Should().Be(payload1.Content);
        record2.Sns.MessageContent.Should().NotBeNull();
        record2.Sns.MessageContent!.Content.Should().Be(payload2.Content);
        record3.Sns.MessageContent.Should().NotBeNull();
        record3.Sns.MessageContent!.Content.Should().Be(payload3.Content);
    }

    [Fact]
    public void ExtractPayload_WithEmptyRecordsList_CompletesWithoutError()
    {
        // Arrange
        var envelope = new SnsEnvelope<MessagePayload> { Records = [] };
        var options = new EnvelopeOptions();

        // Act
        var act = () => envelope.ExtractPayload(options);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ExtractPayload_WithCamelCaseNamingPolicy_DeserializesWithCamelCaseProperties()
    {
        // Arrange
        var payload = _fixture.Create<MessagePayload>();
        var json = JsonSerializer.Serialize(
            payload,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        );
        var record = new SnsEnvelopeBase<MessagePayload>.SnsRecordEnvelope
        {
            Sns = new SnsEnvelopeBase<MessagePayload>.SnsMessageEnvelope { Message = json },
        };
        var envelope = new SnsEnvelope<MessagePayload> { Records = [record] };
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
        record.Sns.MessageContent.Should().NotBeNull();
        record.Sns.MessageContent!.Content.Should().Be(payload.Content);
        record.Sns.MessageContent.Priority.Should().Be(payload.Priority);
    }

    [Fact]
    public void ExtractPayload_WithNullMessage_ThrowsArgumentNullException()
    {
        // Arrange
        var record = new SnsEnvelopeBase<MessagePayload>.SnsRecordEnvelope
        {
            Sns = new SnsEnvelopeBase<MessagePayload>.SnsMessageEnvelope { Message = null },
        };
        var envelope = new SnsEnvelope<MessagePayload> { Records = [record] };
        var options = new EnvelopeOptions();

        // Act & Assert
        var act = () => envelope.ExtractPayload(options);
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void ExtractPayload_WithEmptyMessage_ThrowsJsonException()
    {
        // Arrange
        var record = new SnsEnvelopeBase<MessagePayload>.SnsRecordEnvelope
        {
            Sns = new SnsEnvelopeBase<MessagePayload>.SnsMessageEnvelope { Message = string.Empty },
        };
        var envelope = new SnsEnvelope<MessagePayload> { Records = [record] };
        var options = new EnvelopeOptions();

        // Act & Assert
        var act = () => envelope.ExtractPayload(options);
        act.Should().ThrowExactly<JsonException>();
    }

    [Fact]
    public void SnsMessageEnvalope_MessageContent_HasJsonIgnoreAttribute()
    {
        // Arrange
        var property = typeof(SnsEnvelopeBase<MessagePayload>.SnsMessageEnvelope).GetProperty(
            nameof(SnsEnvelopeBase<MessagePayload>.SnsMessageEnvelope.MessageContent)
        );

        // Act
        var hasJsonIgnoreAttribute =
            property?.GetCustomAttributes(typeof(JsonIgnoreAttribute), false).Length > 0;

        // Assert
        hasJsonIgnoreAttribute.Should().BeTrue();
    }

    [Fact]
    public void SnsEnvelope_InheritsFromSnsEvent()
    {
        // Arrange & Act
        var envelope = new SnsEnvelope<MessagePayload> { Records = [] };

        // Assert
        envelope.Should().BeAssignableTo<SNSEvent>();
    }

    private record MessagePayload(string Content, int Priority);
}
