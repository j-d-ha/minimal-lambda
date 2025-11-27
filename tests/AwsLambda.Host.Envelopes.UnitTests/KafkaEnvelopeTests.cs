using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.KafkaEvents;
using AutoFixture;
using AwesomeAssertions;
using AwsLambda.Host.Envelopes.Kafka;
using AwsLambda.Host.Options;
using JetBrains.Annotations;
using Xunit;

namespace AwsLambda.Host.Envelopes.UnitTests;

[TestSubject(typeof(KafkaEnvelope<>))]
public class KafkaEnvelopeTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void ExtractPayload_WithSingleRecordInSingleTopic_DeserializesValueContent()
    {
        // Arrange
        var payload = _fixture.Create<MessagePayload>();
        var record = CreateKafkaRecord(payload);
        var envelope = new KafkaEnvelope<MessagePayload>
        {
            Records = new Dictionary<
                string,
                IList<KafkaEnvelopeBase<MessagePayload>.KafkaEventRecordEnvelope>
            >
            {
                ["test-topic"] =
                    new List<KafkaEnvelopeBase<MessagePayload>.KafkaEventRecordEnvelope> { record },
            },
        };
        var options = new EnvelopeOptions();

        // Act
        envelope.ExtractPayload(options);

        // Assert
        record.ValueContent.Should().NotBeNull();
        record.ValueContent!.Content.Should().Be(payload.Content);
        record.ValueContent.Priority.Should().Be(payload.Priority);
    }

    [Fact]
    public void ExtractPayload_WithMultipleRecordsInSingleTopic_DeserializesAllRecords()
    {
        // Arrange
        var payload1 = _fixture.Create<MessagePayload>();
        var payload2 = _fixture.Create<MessagePayload>();
        var payload3 = _fixture.Create<MessagePayload>();

        var record1 = CreateKafkaRecord(payload1);
        var record2 = CreateKafkaRecord(payload2);
        var record3 = CreateKafkaRecord(payload3);

        var envelope = new KafkaEnvelope<MessagePayload>
        {
            Records = new Dictionary<
                string,
                IList<KafkaEnvelopeBase<MessagePayload>.KafkaEventRecordEnvelope>
            >
            {
                ["test-topic"] =
                    new List<KafkaEnvelopeBase<MessagePayload>.KafkaEventRecordEnvelope>
                    {
                        record1,
                        record2,
                        record3,
                    },
            },
        };
        var options = new EnvelopeOptions();

        // Act
        envelope.ExtractPayload(options);

        // Assert
        record1.ValueContent.Should().NotBeNull();
        record1.ValueContent!.Content.Should().Be(payload1.Content);
        record2.ValueContent.Should().NotBeNull();
        record2.ValueContent!.Content.Should().Be(payload2.Content);
        record3.ValueContent.Should().NotBeNull();
        record3.ValueContent!.Content.Should().Be(payload3.Content);
    }

    [Fact]
    public void ExtractPayload_WithMultipleTopics_DeserializesAllRecordsAcrossAllTopics()
    {
        // Arrange
        var payload1 = _fixture.Create<MessagePayload>();
        var payload2 = _fixture.Create<MessagePayload>();
        var payload3 = _fixture.Create<MessagePayload>();

        var record1 = CreateKafkaRecord(payload1);
        var record2 = CreateKafkaRecord(payload2);
        var record3 = CreateKafkaRecord(payload3);

        var envelope = new KafkaEnvelope<MessagePayload>
        {
            Records = new Dictionary<
                string,
                IList<KafkaEnvelopeBase<MessagePayload>.KafkaEventRecordEnvelope>
            >
            {
                ["topic-1"] = new List<KafkaEnvelopeBase<MessagePayload>.KafkaEventRecordEnvelope>
                {
                    record1,
                    record2,
                },
                ["topic-2"] = new List<KafkaEnvelopeBase<MessagePayload>.KafkaEventRecordEnvelope>
                {
                    record3,
                },
            },
        };
        var options = new EnvelopeOptions();

        // Act
        envelope.ExtractPayload(options);

        // Assert
        record1.ValueContent.Should().NotBeNull();
        record1.ValueContent!.Content.Should().Be(payload1.Content);
        record2.ValueContent.Should().NotBeNull();
        record2.ValueContent!.Content.Should().Be(payload2.Content);
        record3.ValueContent.Should().NotBeNull();
        record3.ValueContent!.Content.Should().Be(payload3.Content);
    }

    [Fact]
    public void ExtractPayload_WithEmptyRecordsDictionary_CompletesWithoutError()
    {
        // Arrange
        var envelope = new KafkaEnvelope<MessagePayload>
        {
            Records =
                new Dictionary<
                    string,
                    IList<KafkaEnvelopeBase<MessagePayload>.KafkaEventRecordEnvelope>
                >(),
        };
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
        var base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        var valueStream = new MemoryStream(Encoding.UTF8.GetBytes(base64String));

        var record = new KafkaEnvelopeBase<MessagePayload>.KafkaEventRecordEnvelope
        {
            Value = valueStream,
        };
        var envelope = new KafkaEnvelope<MessagePayload>
        {
            Records = new Dictionary<
                string,
                IList<KafkaEnvelopeBase<MessagePayload>.KafkaEventRecordEnvelope>
            >
            {
                ["test-topic"] =
                    new List<KafkaEnvelopeBase<MessagePayload>.KafkaEventRecordEnvelope> { record },
            },
        };
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
        record.ValueContent.Should().NotBeNull();
        record.ValueContent!.Content.Should().Be(payload.Content);
        record.ValueContent.Priority.Should().Be(payload.Priority);
    }

    [Fact]
    public void ExtractPayload_WithNullValue_ThrowsArgumentNullException()
    {
        // Arrange
        var record = new KafkaEnvelopeBase<MessagePayload>.KafkaEventRecordEnvelope
        {
            Value = null,
        };
        var envelope = new KafkaEnvelope<MessagePayload>
        {
            Records = new Dictionary<
                string,
                IList<KafkaEnvelopeBase<MessagePayload>.KafkaEventRecordEnvelope>
            >
            {
                ["test-topic"] =
                    new List<KafkaEnvelopeBase<MessagePayload>.KafkaEventRecordEnvelope> { record },
            },
        };
        var options = new EnvelopeOptions();

        // Act & Assert
        var act = () => envelope.ExtractPayload(options);
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void ExtractPayload_WithInvalidJson_ThrowsJsonException()
    {
        // Arrange
        var invalidJson = "not valid json";
        var base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(invalidJson));
        var valueStream = new MemoryStream(Encoding.UTF8.GetBytes(base64String));

        var record = new KafkaEnvelopeBase<MessagePayload>.KafkaEventRecordEnvelope
        {
            Value = valueStream,
        };
        var envelope = new KafkaEnvelope<MessagePayload>
        {
            Records = new Dictionary<
                string,
                IList<KafkaEnvelopeBase<MessagePayload>.KafkaEventRecordEnvelope>
            >
            {
                ["test-topic"] =
                    new List<KafkaEnvelopeBase<MessagePayload>.KafkaEventRecordEnvelope> { record },
            },
        };
        var options = new EnvelopeOptions();

        // Act & Assert
        var act = () => envelope.ExtractPayload(options);
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void KafkaEventRecordEnvelope_ValueContent_HasJsonIgnoreAttribute()
    {
        // Arrange
        var property =
            typeof(KafkaEnvelopeBase<MessagePayload>.KafkaEventRecordEnvelope).GetProperty(
                nameof(KafkaEnvelopeBase<MessagePayload>.KafkaEventRecordEnvelope.ValueContent)
            );

        // Act
        var hasJsonIgnoreAttribute =
            property?.GetCustomAttributes(typeof(JsonIgnoreAttribute), false).Length > 0;

        // Assert
        hasJsonIgnoreAttribute.Should().BeTrue();
    }

    [Fact]
    public void KafkaEnvelope_InheritsFromKafkaEvent()
    {
        // Arrange & Act
        var envelope = new KafkaEnvelope<MessagePayload>
        {
            Records =
                new Dictionary<
                    string,
                    IList<KafkaEnvelopeBase<MessagePayload>.KafkaEventRecordEnvelope>
                >(),
        };

        // Assert
        envelope.Should().BeAssignableTo<KafkaEvent>();
    }

    private static KafkaEnvelopeBase<T>.KafkaEventRecordEnvelope CreateKafkaRecord<T>(T payload)
    {
        var json = JsonSerializer.Serialize(payload);
        var base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        var valueStream = new MemoryStream(Encoding.UTF8.GetBytes(base64String));

        return new KafkaEnvelopeBase<T>.KafkaEventRecordEnvelope { Value = valueStream };
    }

    private record MessagePayload(string Content, int Priority);
}
