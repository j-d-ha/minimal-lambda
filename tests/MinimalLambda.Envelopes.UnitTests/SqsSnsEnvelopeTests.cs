using System.Text.Json;
using AutoFixture;
using AwesomeAssertions;
using JetBrains.Annotations;
using MinimalLambda.Envelopes.Sns;
using MinimalLambda.Envelopes.Sqs;
using MinimalLambda.Options;
using Xunit;

namespace MinimalLambda.Envelopes.UnitTests;

[TestSubject(typeof(SqsSnsEnvelope<>))]
public class SqsSnsEnvelopeTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void ExtractPayload_WithSingleRecord_DeserializesTwoStagePayload()
    {
        // Arrange
        var payload = _fixture.Create<MessagePayload>();
        var innerJson = JsonSerializer.Serialize(payload);
        var snsEnvelope = new { Message = innerJson };
        var outerJson = JsonSerializer.Serialize(snsEnvelope);

        var record =
            new SqsEnvelopeBase<SnsEnvelopeBase<MessagePayload>.SnsMessageEnvelope>.
                SqsMessageEnvelope { Body = outerJson };
        var envelope = new SqsSnsEnvelope<MessagePayload> { Records = [record] };
        var options = new EnvelopeOptions();

        // Act
        envelope.ExtractPayload(options);

        // Assert
        record.BodyContent.Should().NotBeNull();
        record.BodyContent!.MessageContent.Should().NotBeNull();
        record.BodyContent.MessageContent!.Content.Should().Be(payload.Content);
        record.BodyContent.MessageContent.Priority.Should().Be(payload.Priority);
    }

    [Fact]
    public void ExtractPayload_WithMultipleRecords_DeserializesAllTwoStagePayloads()
    {
        // Arrange
        var payload1 = _fixture.Create<MessagePayload>();
        var payload2 = _fixture.Create<MessagePayload>();
        var payload3 = _fixture.Create<MessagePayload>();

        var innerJson1 = JsonSerializer.Serialize(payload1);
        var innerJson2 = JsonSerializer.Serialize(payload2);
        var innerJson3 = JsonSerializer.Serialize(payload3);

        var snsEnvelope1 = new { Message = innerJson1 };
        var snsEnvelope2 = new { Message = innerJson2 };
        var snsEnvelope3 = new { Message = innerJson3 };

        var outerJson1 = JsonSerializer.Serialize(snsEnvelope1);
        var outerJson2 = JsonSerializer.Serialize(snsEnvelope2);
        var outerJson3 = JsonSerializer.Serialize(snsEnvelope3);

        var record1 =
            new SqsEnvelopeBase<SnsEnvelopeBase<MessagePayload>.SnsMessageEnvelope>.
                SqsMessageEnvelope { Body = outerJson1 };
        var record2 =
            new SqsEnvelopeBase<SnsEnvelopeBase<MessagePayload>.SnsMessageEnvelope>.
                SqsMessageEnvelope { Body = outerJson2 };
        var record3 =
            new SqsEnvelopeBase<SnsEnvelopeBase<MessagePayload>.SnsMessageEnvelope>.
                SqsMessageEnvelope { Body = outerJson3 };

        var envelope = new SqsSnsEnvelope<MessagePayload> { Records = [record1, record2, record3] };
        var options = new EnvelopeOptions();

        // Act
        envelope.ExtractPayload(options);

        // Assert
        record1.BodyContent.Should().NotBeNull();
        record1.BodyContent!.MessageContent.Should().NotBeNull();
        record1.BodyContent.MessageContent!.Content.Should().Be(payload1.Content);

        record2.BodyContent.Should().NotBeNull();
        record2.BodyContent!.MessageContent.Should().NotBeNull();
        record2.BodyContent.MessageContent!.Content.Should().Be(payload2.Content);

        record3.BodyContent.Should().NotBeNull();
        record3.BodyContent!.MessageContent.Should().NotBeNull();
        record3.BodyContent.MessageContent!.Content.Should().Be(payload3.Content);
    }

    [Fact]
    public void ExtractPayload_WithEmptyRecordsList_CompletesWithoutError()
    {
        // Arrange
        var envelope = new SqsSnsEnvelope<MessagePayload> { Records = [] };
        var options = new EnvelopeOptions();

        // Act
        var act = () => envelope.ExtractPayload(options);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ExtractPayload_WithCustomJsonOptions_DeserializesInnerPayloadCorrectly()
    {
        // Arrange
        var payload = _fixture.Create<MessagePayload>();
        var innerJson = JsonSerializer.Serialize(
            payload,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var snsEnvelope = new { Message = innerJson };
        var outerJson = JsonSerializer.Serialize(snsEnvelope);

        var record =
            new SqsEnvelopeBase<SnsEnvelopeBase<MessagePayload>.SnsMessageEnvelope>.
                SqsMessageEnvelope { Body = outerJson };
        var envelope = new SqsSnsEnvelope<MessagePayload> { Records = [record] };
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
        record.BodyContent.Should().NotBeNull();
        record.BodyContent!.MessageContent.Should().NotBeNull();
        record.BodyContent.MessageContent!.Content.Should().Be(payload.Content);
        record.BodyContent.MessageContent.Priority.Should().Be(payload.Priority);
    }

    [Fact]
    public void ExtractPayload_WithNullSqsBody_ThrowsArgumentNullException()
    {
        // Arrange
        var record =
            new SqsEnvelopeBase<SnsEnvelopeBase<MessagePayload>.SnsMessageEnvelope>.
                SqsMessageEnvelope { Body = null };
        var envelope = new SqsSnsEnvelope<MessagePayload> { Records = [record] };
        var options = new EnvelopeOptions();

        // Act & Assert
        var act = () => envelope.ExtractPayload(options);
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void ExtractPayload_WithInvalidSnsEnvelopeJson_ThrowsJsonException()
    {
        // Arrange
        var invalidJson = _fixture.Create<string>();
        var record =
            new SqsEnvelopeBase<SnsEnvelopeBase<MessagePayload>.SnsMessageEnvelope>.
                SqsMessageEnvelope { Body = invalidJson };
        var envelope = new SqsSnsEnvelope<MessagePayload> { Records = [record] };
        var options = new EnvelopeOptions();

        // Act & Assert
        var act = () => envelope.ExtractPayload(options);
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void ExtractPayload_WithNullSnsMessage_ThrowsArgumentNullException()
    {
        // Arrange
        var snsEnvelope = new { Message = (string?)null };
        var outerJson = JsonSerializer.Serialize(snsEnvelope);

        var record =
            new SqsEnvelopeBase<SnsEnvelopeBase<MessagePayload>.SnsMessageEnvelope>.
                SqsMessageEnvelope { Body = outerJson };
        var envelope = new SqsSnsEnvelope<MessagePayload> { Records = [record] };
        var options = new EnvelopeOptions();

        // Act & Assert
        var act = () => envelope.ExtractPayload(options);
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void ExtractPayload_WithInvalidInnerPayloadJson_ThrowsJsonException()
    {
        // Arrange
        var invalidInnerJson = _fixture.Create<string>();
        var snsEnvelope = new { Message = invalidInnerJson };
        var outerJson = JsonSerializer.Serialize(snsEnvelope);

        var record =
            new SqsEnvelopeBase<SnsEnvelopeBase<MessagePayload>.SnsMessageEnvelope>.
                SqsMessageEnvelope { Body = outerJson };
        var envelope = new SqsSnsEnvelope<MessagePayload> { Records = [record] };
        var options = new EnvelopeOptions();

        // Act & Assert
        var act = () => envelope.ExtractPayload(options);
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void SqsSnsEnvelope_InheritsFromSqsEnvelopeBase()
    {
        // Arrange & Act
        var envelope = new SqsSnsEnvelope<MessagePayload> { Records = [] };

        // Assert
        envelope
            .Should()
            .BeAssignableTo<SqsEnvelopeBase<SnsEnvelopeBase<MessagePayload>.SnsMessageEnvelope>>();
    }

    [Fact]
    public void ExtractPayload_WithValidNullInnerPayload_SetsMessageContentToNull()
    {
        // Arrange
        var innerJson = "null";
        var snsEnvelope = new { Message = innerJson };
        var outerJson = JsonSerializer.Serialize(snsEnvelope);

        var record =
            new SqsEnvelopeBase<SnsEnvelopeBase<MessagePayload>.SnsMessageEnvelope>.
                SqsMessageEnvelope { Body = outerJson };
        var envelope = new SqsSnsEnvelope<MessagePayload> { Records = [record] };
        var options = new EnvelopeOptions();

        // Act
        envelope.ExtractPayload(options);

        // Assert
        record.BodyContent.Should().NotBeNull();
        record.BodyContent!.MessageContent.Should().BeNull();
    }

    private record MessagePayload(string Content, int Priority);
}
