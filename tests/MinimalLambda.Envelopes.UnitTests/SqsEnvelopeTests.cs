using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.SQSEvents;
using AutoFixture;
using AwesomeAssertions;
using AwsLambda.Host.Options;
using JetBrains.Annotations;
using MinimalLambda.Envelopes.Sqs;
using Xunit;
using SQSMessage = Amazon.Lambda.SQSEvents.SQSEvent.SQSMessage;

namespace MinimalLambda.Envelopes.UnitTests;

[TestSubject(typeof(SqsEnvelope<>))]
public class SqsEnvelopeTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void ExtractPayload_WithSingleRecord_DeserializesBodyContent()
    {
        // Arrange
        var payload = _fixture.Create<MessagePayload>();
        var json = JsonSerializer.Serialize(payload);
        var record = new SqsEnvelopeBase<MessagePayload>.SqsMessageEnvelope { Body = json };
        var envelope = new SqsEnvelope<MessagePayload> { Records = [record] };
        var options = new EnvelopeOptions();

        // Act
        envelope.ExtractPayload(options);

        // Assert
        record.BodyContent.Should().NotBeNull();
        record.BodyContent!.Content.Should().Be(payload.Content);
        record.BodyContent.Priority.Should().Be(payload.Priority);
    }

    [Fact]
    public void ExtractPayload_WithMultipleRecords_DeserializesAllBodies()
    {
        // Arrange
        var payload1 = _fixture.Create<MessagePayload>();
        var payload2 = _fixture.Create<MessagePayload>();
        var payload3 = _fixture.Create<MessagePayload>();
        var json1 = JsonSerializer.Serialize(payload1);
        var json2 = JsonSerializer.Serialize(payload2);
        var json3 = JsonSerializer.Serialize(payload3);

        var record1 = new SqsEnvelopeBase<MessagePayload>.SqsMessageEnvelope { Body = json1 };
        var record2 = new SqsEnvelopeBase<MessagePayload>.SqsMessageEnvelope { Body = json2 };
        var record3 = new SqsEnvelopeBase<MessagePayload>.SqsMessageEnvelope { Body = json3 };

        var envelope = new SqsEnvelope<MessagePayload> { Records = [record1, record2, record3] };
        var options = new EnvelopeOptions();

        // Act
        envelope.ExtractPayload(options);

        // Assert
        record1.BodyContent.Should().NotBeNull();
        record1.BodyContent!.Content.Should().Be(payload1.Content);
        record2.BodyContent.Should().NotBeNull();
        record2.BodyContent!.Content.Should().Be(payload2.Content);
        record3.BodyContent.Should().NotBeNull();
        record3.BodyContent!.Content.Should().Be(payload3.Content);
    }

    [Fact]
    public void ExtractPayload_WithEmptyRecordsList_CompletesWithoutError()
    {
        // Arrange
        var envelope = new SqsEnvelope<MessagePayload> { Records = [] };
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
        var record = new SqsEnvelopeBase<MessagePayload>.SqsMessageEnvelope { Body = json };
        var envelope = new SqsEnvelope<MessagePayload> { Records = [record] };
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
        record.BodyContent!.Content.Should().Be(payload.Content);
        record.BodyContent.Priority.Should().Be(payload.Priority);
    }

    [Fact]
    public void ExtractPayload_WithNullBody_ThrowsArgumentNullException()
    {
        // Arrange
        var record = new SqsEnvelopeBase<MessagePayload>.SqsMessageEnvelope { Body = null };
        var envelope = new SqsEnvelope<MessagePayload> { Records = [record] };
        var options = new EnvelopeOptions();

        // Act & Assert
        var act = () => envelope.ExtractPayload(options);
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void ExtractPayload_WithEmptyBody_ThrowsJsonException()
    {
        // Arrange
        var record = new SqsEnvelopeBase<MessagePayload>.SqsMessageEnvelope { Body = string.Empty };
        var envelope = new SqsEnvelope<MessagePayload> { Records = [record] };
        var options = new EnvelopeOptions();

        // Act & Assert
        var act = () => envelope.ExtractPayload(options);
        act.Should().ThrowExactly<JsonException>();
    }

    [Fact]
    public void ExtractPayload_WithInvalidJson_ThrowsJsonException()
    {
        // Arrange
        var invalidJson = _fixture.Create<string>();
        var record = new SqsEnvelopeBase<MessagePayload>.SqsMessageEnvelope { Body = invalidJson };
        var envelope = new SqsEnvelope<MessagePayload> { Records = [record] };
        var options = new EnvelopeOptions();

        // Act & Assert
        var act = () => envelope.ExtractPayload(options);
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void ExtractPayload_WithMalformedJsonStructure_ThrowsJsonException()
    {
        // Arrange
        var malformedJson = """{"Content":"Valid","Priority":"NotAnInt"}""";
        var record = new SqsEnvelopeBase<MessagePayload>.SqsMessageEnvelope
        {
            Body = malformedJson,
        };
        var envelope = new SqsEnvelope<MessagePayload> { Records = [record] };
        var options = new EnvelopeOptions();

        // Act & Assert
        var act = () => envelope.ExtractPayload(options);
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void ExtractPayload_WithValidNullValue_SetsBodyContentToNull()
    {
        // Arrange
        var record = new SqsEnvelopeBase<MessagePayload>.SqsMessageEnvelope { Body = "null" };
        var envelope = new SqsEnvelope<MessagePayload> { Records = [record] };
        var options = new EnvelopeOptions();

        // Act
        envelope.ExtractPayload(options);

        // Assert
        record.BodyContent.Should().BeNull();
    }

    [Fact]
    public void SqsMessageEnvelope_BodyContent_HasJsonIgnoreAttribute()
    {
        // Arrange
        var property = typeof(SqsEnvelopeBase<MessagePayload>.SqsMessageEnvelope).GetProperty(
            nameof(SqsEnvelopeBase<MessagePayload>.SqsMessageEnvelope.BodyContent)
        );

        // Act
        var hasJsonIgnoreAttribute =
            property?.GetCustomAttributes(typeof(JsonIgnoreAttribute), false).Length > 0;

        // Assert
        hasJsonIgnoreAttribute.Should().BeTrue();
    }

    [Fact]
    public void SqsEnvelope_InheritsFromSqsEvent()
    {
        // Arrange & Act
        var envelope = new SqsEnvelope<MessagePayload> { Records = [] };

        // Assert
        envelope.Should().BeAssignableTo<SQSEvent>();
    }

    [Fact]
    public void SqsMessageEnvelope_InheritsFromSqsMessage()
    {
        // Arrange & Act
        var messageEnvelope = new SqsEnvelopeBase<MessagePayload>.SqsMessageEnvelope();

        // Assert
        messageEnvelope.Should().BeAssignableTo<SQSMessage>();
    }

    [Fact]
    public void ExtractPayload_WithMixedValidAndInvalidRecords_StopsAtFirstError()
    {
        // Arrange
        var validPayload = _fixture.Create<MessagePayload>();
        var validJson = JsonSerializer.Serialize(validPayload);
        var validRecord = new SqsEnvelopeBase<MessagePayload>.SqsMessageEnvelope
        {
            Body = validJson,
        };
        var invalidRecord = new SqsEnvelopeBase<MessagePayload>.SqsMessageEnvelope
        {
            Body = "invalid",
        };

        var envelope = new SqsEnvelope<MessagePayload> { Records = [validRecord, invalidRecord] };
        var options = new EnvelopeOptions();

        // Act & Assert
        var act = () => envelope.ExtractPayload(options);
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void ExtractPayload_WithLargeNumberOfRecords_DeserializesAllSuccessfully()
    {
        // Arrange
        const int recordCount = 100;
        var records = new List<SqsEnvelopeBase<MessagePayload>.SqsMessageEnvelope>();

        for (var i = 0; i < recordCount; i++)
        {
            var payload = new MessagePayload($"Message{i}", i);
            var json = JsonSerializer.Serialize(payload);
            records.Add(new SqsEnvelopeBase<MessagePayload>.SqsMessageEnvelope { Body = json });
        }

        var envelope = new SqsEnvelope<MessagePayload> { Records = records };
        var options = new EnvelopeOptions();

        // Act
        envelope.ExtractPayload(options);

        // Assert
        envelope.Records.Should().HaveCount(recordCount);
        for (var i = 0; i < recordCount; i++)
        {
            var record = envelope.Records[i];
            record.BodyContent.Should().NotBeNull();
            record.BodyContent!.Content.Should().Be($"Message{i}");
            record.BodyContent.Priority.Should().Be(i);
        }
    }

    private record MessagePayload(string Content, int Priority);
}
