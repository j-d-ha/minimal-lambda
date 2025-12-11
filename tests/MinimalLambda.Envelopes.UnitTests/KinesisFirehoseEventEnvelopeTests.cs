using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.KinesisFirehoseEvents;
using AutoFixture;
using AwesomeAssertions;
using AwsLambda.Host.Options;
using JetBrains.Annotations;
using MinimalLambda.Envelopes.KinesisFirehose;
using Xunit;

namespace MinimalLambda.Envelopes.UnitTests;

[TestSubject(typeof(KinesisFirehoseEventEnvelope<>))]
public class KinesisFirehoseEventEnvelopeTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void ExtractPayload_WithSingleRecord_DeserializesDataContent()
    {
        // Arrange
        var payload = _fixture.Create<TestPayload>();
        var record = CreateFirehoseRecord(payload);
        var envelope = new KinesisFirehoseEventEnvelope<TestPayload> { Records = [record] };
        var options = new EnvelopeOptions();

        // Act
        envelope.ExtractPayload(options);

        // Assert
        record.DataContent.Should().NotBeNull();
        record.DataContent!.Name.Should().Be(payload.Name);
        record.DataContent.Value.Should().Be(payload.Value);
    }

    [Fact]
    public void ExtractPayload_WithMultipleRecords_DeserializesAllRecords()
    {
        // Arrange
        var payload1 = _fixture.Create<TestPayload>();
        var payload2 = _fixture.Create<TestPayload>();
        var payload3 = _fixture.Create<TestPayload>();

        var record1 = CreateFirehoseRecord(payload1);
        var record2 = CreateFirehoseRecord(payload2);
        var record3 = CreateFirehoseRecord(payload3);

        var envelope = new KinesisFirehoseEventEnvelope<TestPayload>
        {
            Records = [record1, record2, record3],
        };
        var options = new EnvelopeOptions();

        // Act
        envelope.ExtractPayload(options);

        // Assert
        record1.DataContent.Should().NotBeNull();
        record1.DataContent!.Name.Should().Be(payload1.Name);
        record2.DataContent.Should().NotBeNull();
        record2.DataContent!.Name.Should().Be(payload2.Name);
        record3.DataContent.Should().NotBeNull();
        record3.DataContent!.Name.Should().Be(payload3.Name);
    }

    [Fact]
    public void ExtractPayload_WithEmptyRecordsList_CompletesWithoutError()
    {
        // Arrange
        var envelope = new KinesisFirehoseEventEnvelope<TestPayload> { Records = [] };
        var options = new EnvelopeOptions();

        // Act
        var act = () => envelope.ExtractPayload(options);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ExtractPayload_WithCamelCaseNamingPolicy_DeserializesCorrectly()
    {
        // Arrange
        var payload = _fixture.Create<TestPayload>();
        var json = JsonSerializer.Serialize(
            payload,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        );
        var base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

        var record = new KinesisFirehoseEventEnvelopeBase<TestPayload>.FirehoseRecordEnvelope
        {
            Base64EncodedData = base64String,
        };

        var envelope = new KinesisFirehoseEventEnvelope<TestPayload> { Records = [record] };
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
        record.DataContent.Should().NotBeNull();
        record.DataContent!.Name.Should().Be(payload.Name);
        record.DataContent.Value.Should().Be(payload.Value);
    }

    [Fact]
    public void DataContent_HasJsonIgnoreAttribute()
    {
        // Arrange
        var property =
            typeof(KinesisFirehoseEventEnvelopeBase<TestPayload>.FirehoseRecordEnvelope).GetProperty(
                nameof(
                    KinesisFirehoseEventEnvelopeBase<TestPayload>.FirehoseRecordEnvelope.DataContent
                )
            );

        // Act
        var hasJsonIgnoreAttribute =
            property?.GetCustomAttributes(typeof(JsonIgnoreAttribute), false).Length > 0;

        // Assert
        hasJsonIgnoreAttribute.Should().BeTrue();
    }

    [Fact]
    public void KinesisFirehoseEventEnvelope_InheritsFromKinesisFirehoseEvent()
    {
        // Arrange & Act
        var envelope = new KinesisFirehoseEventEnvelope<TestPayload> { Records = [] };

        // Assert
        envelope.Should().BeAssignableTo<KinesisFirehoseEvent>();
    }

    private static KinesisFirehoseEventEnvelopeBase<T>.FirehoseRecordEnvelope CreateFirehoseRecord<T>(
        T payload
    )
    {
        var json = JsonSerializer.Serialize(payload);
        var base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

        return new KinesisFirehoseEventEnvelopeBase<T>.FirehoseRecordEnvelope
        {
            Base64EncodedData = base64String,
        };
    }

    private record TestPayload(string Name, int Value);
}
