using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.KinesisFirehoseEvents;
using AutoFixture;
using AwesomeAssertions;
using JetBrains.Annotations;
using MinimalLambda.Envelopes.KinesisFirehose;
using MinimalLambda.Options;
using Xunit;

namespace MinimalLambda.Envelopes.UnitTests;

[TestSubject(typeof(KinesisFirehoseResponseEnvelope<>))]
public class KinesisFirehoseResponseEnvelopeTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void PackPayload_WithValidData_SerializesDataContent()
    {
        // Arrange
        var payload = _fixture.Create<TestPayload>();
        var record = new KinesisFirehoseResponseEnvelopeBase<TestPayload>.FirehoseRecordEnvelope
        {
            RecordId = "record-1",
            Result = "Ok",
            DataContent = payload,
        };

        var envelope = new KinesisFirehoseResponseEnvelope<TestPayload> { Records = [record] };
        var options = new EnvelopeOptions();

        // Act
        envelope.PackPayload(options);

        // Assert
        record.Base64EncodedData.Should().NotBeNullOrEmpty();
        var decodedJson = Encoding.UTF8.GetString(
            Convert.FromBase64String(record.Base64EncodedData)
        );
        var deserializedData = JsonSerializer.Deserialize<TestPayload>(
            decodedJson,
            options.JsonOptions
        );
        deserializedData.Should().NotBeNull();
        deserializedData.Name.Should().Be(payload.Name);
        deserializedData.Value.Should().Be(payload.Value);
    }

    [Fact]
    public void PackPayload_WithMultipleRecords_SerializesAllRecords()
    {
        // Arrange
        var payload1 = _fixture.Create<TestPayload>();
        var payload2 = _fixture.Create<TestPayload>();
        var payload3 = _fixture.Create<TestPayload>();

        var record1 = new KinesisFirehoseResponseEnvelopeBase<TestPayload>.FirehoseRecordEnvelope
        {
            RecordId = "record-1",
            Result = "Ok",
            DataContent = payload1,
        };
        var record2 = new KinesisFirehoseResponseEnvelopeBase<TestPayload>.FirehoseRecordEnvelope
        {
            RecordId = "record-2",
            Result = "Ok",
            DataContent = payload2,
        };
        var record3 = new KinesisFirehoseResponseEnvelopeBase<TestPayload>.FirehoseRecordEnvelope
        {
            RecordId = "record-3",
            Result = "Ok",
            DataContent = payload3,
        };

        var envelope = new KinesisFirehoseResponseEnvelope<TestPayload>
        {
            Records = [record1, record2, record3],
        };
        var options = new EnvelopeOptions();

        // Act
        envelope.PackPayload(options);

        // Assert
        record1.Base64EncodedData.Should().NotBeNullOrEmpty();
        record2.Base64EncodedData.Should().NotBeNullOrEmpty();
        record3.Base64EncodedData.Should().NotBeNullOrEmpty();

        var decoded1 = JsonSerializer.Deserialize<TestPayload>(
            Encoding.UTF8.GetString(Convert.FromBase64String(record1.Base64EncodedData))
        );
        var decoded2 = JsonSerializer.Deserialize<TestPayload>(
            Encoding.UTF8.GetString(Convert.FromBase64String(record2.Base64EncodedData))
        );
        var decoded3 = JsonSerializer.Deserialize<TestPayload>(
            Encoding.UTF8.GetString(Convert.FromBase64String(record3.Base64EncodedData))
        );

        decoded1!.Name.Should().Be(payload1.Name);
        decoded2!.Name.Should().Be(payload2.Name);
        decoded3!.Name.Should().Be(payload3.Name);
    }

    [Fact]
    public void PackPayload_WithCamelCaseNamingPolicy_SerializesWithCamelCaseKeys()
    {
        // Arrange
        var payload = new TestPayload("test", 42);
        var record = new KinesisFirehoseResponseEnvelopeBase<TestPayload>.FirehoseRecordEnvelope
        {
            RecordId = "record-1",
            Result = "Ok",
            DataContent = payload,
        };

        var envelope = new KinesisFirehoseResponseEnvelope<TestPayload> { Records = [record] };
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
        record.Base64EncodedData.Should().NotBeNullOrEmpty();
        var decodedJson = Encoding.UTF8.GetString(
            Convert.FromBase64String(record.Base64EncodedData)
        );
        decodedJson.Should().Contain("\"name\"");
        decodedJson.Should().Contain("\"value\"");
        decodedJson.Should().NotContain("\"Name\"");
        decodedJson.Should().NotContain("\"Value\"");
    }

    [Fact]
    public void PackPayload_WithNullDataContent_SerializesNull()
    {
        // Arrange
        var record = new KinesisFirehoseResponseEnvelopeBase<TestPayload>.FirehoseRecordEnvelope
        {
            RecordId = "record-1",
            Result = "Ok",
            DataContent = null,
        };

        var envelope = new KinesisFirehoseResponseEnvelope<TestPayload> { Records = [record] };
        var options = new EnvelopeOptions();

        // Act
        envelope.PackPayload(options);

        // Assert
        record.Base64EncodedData.Should().NotBeNullOrEmpty();
        var decodedJson = Encoding.UTF8.GetString(
            Convert.FromBase64String(record.Base64EncodedData)
        );
        decodedJson.Should().Be("null");
    }

    [Fact]
    public void DataContent_HasJsonIgnoreAttribute()
    {
        // Arrange
        var property =
            typeof(KinesisFirehoseResponseEnvelopeBase<TestPayload>.FirehoseRecordEnvelope).GetProperty(
                nameof(
                    KinesisFirehoseResponseEnvelopeBase<TestPayload>
                        .FirehoseRecordEnvelope
                        .DataContent
                )
            );

        // Act
        var hasJsonIgnoreAttribute =
            property?.GetCustomAttributes(typeof(JsonIgnoreAttribute), false).Length > 0;

        // Assert
        hasJsonIgnoreAttribute.Should().BeTrue();
    }

    [Fact]
    public void KinesisFirehoseResponseEnvelope_InheritsFromKinesisFirehoseResponse()
    {
        // Arrange & Act
        var envelope = new KinesisFirehoseResponseEnvelope<TestPayload> { Records = [] };

        // Assert
        envelope.Should().BeAssignableTo<KinesisFirehoseResponse>();
    }

    private record TestPayload(string Name, int Value);
}
