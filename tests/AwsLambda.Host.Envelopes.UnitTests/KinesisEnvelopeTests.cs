using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.KinesisEvents;
using AutoFixture;
using AwesomeAssertions;
using AwsLambda.Host.Envelopes.Kinesis;
using AwsLambda.Host.Options;
using JetBrains.Annotations;
using Xunit;

namespace AwsLambda.Host.Envelopes.UnitTests;

[TestSubject(typeof(KinesisEnvelope<>))]
public class KinesisEnvelopeTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void ExtractPayload_WithSingleRecord_DeserializesDataContent()
    {
        // Arrange
        var payload = _fixture.Create<MessagePayload>();
        var record = CreateKinesisRecord(payload);
        var envelope = new KinesisEnvelope<MessagePayload> { Records = [record] };
        var options = new EnvelopeOptions();

        // Act
        envelope.ExtractPayload(options);

        // Assert
        record.Kinesis.DataContent.Should().NotBeNull();
        record.Kinesis.DataContent!.Content.Should().Be(payload.Content);
        record.Kinesis.DataContent.Priority.Should().Be(payload.Priority);
    }

    [Fact]
    public void ExtractPayload_WithMultipleRecords_DeserializesAllRecords()
    {
        // Arrange
        var payload1 = _fixture.Create<MessagePayload>();
        var payload2 = _fixture.Create<MessagePayload>();
        var payload3 = _fixture.Create<MessagePayload>();

        var record1 = CreateKinesisRecord(payload1);
        var record2 = CreateKinesisRecord(payload2);
        var record3 = CreateKinesisRecord(payload3);

        var envelope = new KinesisEnvelope<MessagePayload>
        {
            Records = [record1, record2, record3],
        };
        var options = new EnvelopeOptions();

        // Act
        envelope.ExtractPayload(options);

        // Assert
        record1.Kinesis.DataContent.Should().NotBeNull();
        record1.Kinesis.DataContent!.Content.Should().Be(payload1.Content);
        record2.Kinesis.DataContent.Should().NotBeNull();
        record2.Kinesis.DataContent!.Content.Should().Be(payload2.Content);
        record3.Kinesis.DataContent.Should().NotBeNull();
        record3.Kinesis.DataContent!.Content.Should().Be(payload3.Content);
    }

    [Fact]
    public void ExtractPayload_WithEmptyRecordsList_CompletesWithoutError()
    {
        // Arrange
        var envelope = new KinesisEnvelope<MessagePayload> { Records = [] };
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
        var dataStream = new MemoryStream(Encoding.UTF8.GetBytes(base64String));

        var record = new KinesisEnvelopeBase<MessagePayload>.KinesisEventRecordEnvelope
        {
            Kinesis = new KinesisEnvelopeBase<MessagePayload>.RecordEnvelope { Data = dataStream },
        };
        var envelope = new KinesisEnvelope<MessagePayload> { Records = [record] };
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
        record.Kinesis.DataContent.Should().NotBeNull();
        record.Kinesis.DataContent!.Content.Should().Be(payload.Content);
        record.Kinesis.DataContent.Priority.Should().Be(payload.Priority);
    }

    [Fact]
    public void ExtractPayload_WithNullData_ThrowsArgumentNullException()
    {
        // Arrange
        var record = new KinesisEnvelopeBase<MessagePayload>.KinesisEventRecordEnvelope
        {
            Kinesis = new KinesisEnvelopeBase<MessagePayload>.RecordEnvelope { Data = null },
        };
        var envelope = new KinesisEnvelope<MessagePayload> { Records = [record] };
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
        var dataStream = new MemoryStream(Encoding.UTF8.GetBytes(base64String));

        var record = new KinesisEnvelopeBase<MessagePayload>.KinesisEventRecordEnvelope
        {
            Kinesis = new KinesisEnvelopeBase<MessagePayload>.RecordEnvelope { Data = dataStream },
        };
        var envelope = new KinesisEnvelope<MessagePayload> { Records = [record] };
        var options = new EnvelopeOptions();

        // Act & Assert
        var act = () => envelope.ExtractPayload(options);
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void RecordEnvelope_DataContent_HasJsonIgnoreAttribute()
    {
        // Arrange
        var property = typeof(KinesisEnvelopeBase<MessagePayload>.RecordEnvelope).GetProperty(
            nameof(KinesisEnvelopeBase<MessagePayload>.RecordEnvelope.DataContent)
        );

        // Act
        var hasJsonIgnoreAttribute =
            property?.GetCustomAttributes(typeof(JsonIgnoreAttribute), false).Length > 0;

        // Assert
        hasJsonIgnoreAttribute.Should().BeTrue();
    }

    [Fact]
    public void KinesisEnvelope_InheritsFromKinesisEvent()
    {
        // Arrange & Act
        var envelope = new KinesisEnvelope<MessagePayload> { Records = [] };

        // Assert
        envelope.Should().BeAssignableTo<KinesisEvent>();
    }

    private static KinesisEnvelopeBase<T>.KinesisEventRecordEnvelope CreateKinesisRecord<T>(
        T payload
    )
    {
        var json = JsonSerializer.Serialize(payload);
        var base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        var dataStream = new MemoryStream(Encoding.UTF8.GetBytes(base64String));

        return new KinesisEnvelopeBase<T>.KinesisEventRecordEnvelope
        {
            Kinesis = new KinesisEnvelopeBase<T>.RecordEnvelope { Data = dataStream },
        };
    }

    private record MessagePayload(string Content, int Priority);
}
