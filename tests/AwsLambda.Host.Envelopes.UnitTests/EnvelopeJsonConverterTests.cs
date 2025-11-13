using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AwesomeAssertions;
using JetBrains.Annotations;
using Xunit;

namespace AwsLambda.Host.Envelopes.UnitTests;

/// <summary>
///     Unit tests for <see cref="EnvelopeJsonConverter{TEnvelope}" /> abstract class. Uses a
///     concrete test implementation to test the base class functionality.
/// </summary>
[TestSubject(typeof(EnvelopeJsonConverter<>))]
public class EnvelopeJsonConverterTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_CreatesValidInstance()
    {
        // Act
        var converter = new TestEnvelopeConverter();

        // Assert
        converter.Should().NotBeNull();
        converter.GetType().Should().Be(typeof(TestEnvelopeConverter));
    }

    #endregion

    #region Read Tests

    [Fact]
    public void Read_WithValidJson_DeserializesSuccessfully()
    {
        // Arrange
        var converter = new TestEnvelopeConverter();
        var options = new JsonSerializerOptions();
        var envelope = new TestEnvelope { Id = "test-123", Data = "test-data" };
        var json = JsonSerializer.Serialize(envelope, options);
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));

        // Act
        var result = converter.Read(ref reader, typeof(TestEnvelope), options);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("test-123");
        result.Data.Should().Be("test-data");
    }

    [Fact]
    public void Read_CallsReadPayload_WithDeserializedEnvelope()
    {
        // Arrange
        var converter = new TestEnvelopeConverter();
        var options = new JsonSerializerOptions();
        var envelope = new TestEnvelope { Id = "test-456", Data = "payload-data" };
        var json = JsonSerializer.Serialize(envelope, options);
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));

        // Act
        var result = converter.Read(ref reader, typeof(TestEnvelope), options);

        // Assert - ReadPayload was called (verified by custom implementation)
        result.Should().NotBeNull();
        converter.ReadPayloadCalled.Should().BeTrue();
        converter.LastReadPayloadEnvelope.Should().NotBeNull();
    }

    [Fact]
    public void Read_WithCustomNamingPolicy_UsesSeparateOptions()
    {
        // Arrange
        var converter = new TestEnvelopeConverter();
        var optionsWithPolicy = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        var envelope = new TestEnvelope { Id = "test-789", Data = "test" };
        var json = JsonSerializer.Serialize(envelope);
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));

        // Act
        var result = converter.Read(ref reader, typeof(TestEnvelope), optionsWithPolicy);

        // Assert - Should deserialize despite naming policy
        result.Should().NotBeNull();
    }

    #endregion

    #region Write Tests

    [Fact]
    public void Write_WithValidEnvelope_SerializesSuccessfully()
    {
        // Arrange
        var converter = new TestEnvelopeConverter();
        var options = new JsonSerializerOptions();
        var envelope = new TestEnvelope { Id = "write-123", Data = "write-data" };
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act
        converter.Write(writer, envelope, options);
        writer.Flush();

        // Assert
        var json = Encoding.UTF8.GetString(stream.ToArray());
        json.Should().NotBeEmpty();
        json.Should().Contain("write-123");
        json.Should().Contain("write-data");
    }

    [Fact]
    public void Write_CallsWritePayload_WithEnvelope()
    {
        // Arrange
        var converter = new TestEnvelopeConverter();
        var options = new JsonSerializerOptions();
        var envelope = new TestEnvelope { Id = "payload-456", Data = "payload-test" };
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act
        converter.Write(writer, envelope, options);

        // Assert - WritePayload was called
        converter.WritePayloadCalled.Should().BeTrue();
        converter.LastWritePayloadEnvelope.Should().NotBeNull();
        converter.LastWritePayloadEnvelope!.Id.Should().Be("payload-456");
    }

    [Fact]
    public void Write_WithNullEnvelope_SerializesNull()
    {
        // Arrange
        var converter = new TestEnvelopeConverter();
        var options = new JsonSerializerOptions();
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act
        converter.Write(writer, null!, options);
        writer.Flush();

        // Assert
        var json = Encoding.UTF8.GetString(stream.ToArray());
        json.Should().Contain("null");
    }

    [Fact]
    public void Write_WithComplexPayload_SerializesCompleteStructure()
    {
        // Arrange
        var converter = new TestEnvelopeConverter();
        var options = new JsonSerializerOptions();
        var envelope = new TestEnvelope
        {
            Id = "complex-789",
            Data = "complex-data",
            Metadata = new Dictionary<string, string>
            {
                { "key1", "value1" },
                { "key2", "value2" },
            },
        };
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act
        converter.Write(writer, envelope, options);
        writer.Flush();

        // Assert
        var json = Encoding.UTF8.GetString(stream.ToArray());
        json.Should().Contain("complex-789");
        json.Should().Contain("metadata");
    }

    #endregion

    #region GetConverterInstance Tests

    [Fact]
    public void GetConverterInstance_ReturnsValidConverterInstance()
    {
        // Arrange
        var converter = new TestEnvelopeConverter();

        // Act
        var instance = converter.CallGetConverterInstance();

        // Assert
        instance.Should().NotBeNull();
        instance.Should().BeOfType<TestEnvelopeConverter>();
    }

    [Fact]
    public void GetConverterInstance_ReturnsSameType()
    {
        // Arrange
        var converter = new TestEnvelopeConverter();

        // Act
        var instance = converter.CallGetConverterInstance();

        // Assert
        instance!.GetType().Should().Be(converter.GetType());
    }

    #endregion

    #region Round-Trip Serialization Tests

    [Fact]
    public void RoundTrip_SerializeAndDeserialize_PreservesData()
    {
        // Arrange
        var options = new JsonSerializerOptions { Converters = { new TestEnvelopeConverter() } };
        var originalEnvelope = new TestEnvelope { Id = "roundtrip-123", Data = "roundtrip-data" };

        // Act
        var json = JsonSerializer.Serialize(originalEnvelope, options);
        var deserializedEnvelope = JsonSerializer.Deserialize<TestEnvelope>(json, options);

        // Assert
        deserializedEnvelope.Should().NotBeNull();
        deserializedEnvelope!.Id.Should().Be(originalEnvelope.Id);
        deserializedEnvelope.Data.Should().Be(originalEnvelope.Data);
    }

    [Fact]
    public void RoundTrip_ComplexObject_PreservesStructure()
    {
        // Arrange
        var options = new JsonSerializerOptions { Converters = { new TestEnvelopeConverter() } };
        var complexEnvelope = new TestEnvelope
        {
            Id = "complex-roundtrip",
            Data = "complex-data",
            Metadata = new Dictionary<string, string> { { "env", "test" }, { "version", "1.0" } },
        };

        // Act
        var json = JsonSerializer.Serialize(complexEnvelope, options);
        var result = JsonSerializer.Deserialize<TestEnvelope>(json, options);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(complexEnvelope.Id);
        result.Metadata.Should().NotBeNull();
        result.Metadata!.Count.Should().Be(2);
    }

    #endregion

    #region Custom Naming Policy Tests

    [Fact]
    public void Read_WithCamelCaseNamingPolicy_DeserializesCorrectly()
    {
        // Arrange
        var converter = new TestEnvelopeConverter();
        var optionsWithCamelCase = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        // JSON with camelCase properties
        var json = """{"id":"camel-123","data":"camel-data"}""";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));

        // Act
        var result = converter.Read(ref reader, typeof(TestEnvelope), optionsWithCamelCase);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void Write_WithCamelCaseNamingPolicy_DoesNotAffectConverter()
    {
        // Arrange
        var converter = new TestEnvelopeConverter();
        var optionsWithCamelCase = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        var envelope = new TestEnvelope { Id = "policy-test", Data = "test-data" };
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act
        converter.Write(writer, envelope, optionsWithCamelCase);
        writer.Flush();

        // Assert - Should serialize without issues despite naming policy
        var json = Encoding.UTF8.GetString(stream.ToArray());
        json.Should().NotBeEmpty();
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void Read_WithEmptyJson_HandlesGracefully()
    {
        // Arrange
        var converter = new TestEnvelopeConverter();
        var options = new JsonSerializerOptions();
        var json = "{}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));

        // Act
        var result = converter.Read(ref reader, typeof(TestEnvelope), options);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void Write_MultipleConsecutiveWrites_ProducesValidJson()
    {
        // Arrange
        var converter = new TestEnvelopeConverter();
        var options = new JsonSerializerOptions();
        var envelopes = new[]
        {
            new TestEnvelope { Id = "first", Data = "data1" },
            new TestEnvelope { Id = "second", Data = "data2" },
            new TestEnvelope { Id = "third", Data = "data3" },
        };
        using var stream = new MemoryStream();

        // Act
        foreach (var envelope in envelopes)
        {
            using var writer = new Utf8JsonWriter(stream);
            converter.Write(writer, envelope, options);
            writer.Flush();
        }

        // Assert
        stream.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Converter_WithSpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        var converter = new TestEnvelopeConverter();
        var options = new JsonSerializerOptions();
        var envelope = new TestEnvelope
        {
            Id = "special-chars-™",
            Data = "data with \"quotes\" and \\slashes\\",
        };
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act
        converter.Write(writer, envelope, options);
        writer.Flush();
        var json = Encoding.UTF8.GetString(stream.ToArray());

        // Assert
        json.Should().NotBeEmpty();
    }

    #endregion

    #region Test Helpers

    /// <summary>
    ///     Concrete implementation of <see cref="EnvelopeJsonConverter{TEnvelope}" /> for testing.
    ///     Tracks calls to abstract methods for verification.
    /// </summary>
    private class TestEnvelopeConverter : EnvelopeJsonConverter<TestEnvelope>
    {
        public bool ReadPayloadCalled { get; private set; }
        public bool WritePayloadCalled { get; private set; }
        public TestEnvelope? LastReadPayloadEnvelope { get; private set; }
        public TestEnvelope? LastWritePayloadEnvelope { get; private set; }

        protected override JsonConverter GetConverterInstance() => this;

        /// <summary>
        ///     Public helper method to call the protected GetConverterInstance method. Used for testing
        ///     the method's behavior.
        /// </summary>
        public JsonConverter CallGetConverterInstance() => GetConverterInstance();

        protected override void ReadPayload(TestEnvelope envelope, JsonSerializerOptions options)
        {
            ReadPayloadCalled = true;
            LastReadPayloadEnvelope = envelope;
        }

        protected override void WritePayload(TestEnvelope envelope, JsonSerializerOptions options)
        {
            WritePayloadCalled = true;
            LastWritePayloadEnvelope = envelope;
        }
    }

    /// <summary>Simple test model for envelope conversion testing.</summary>
    private class TestEnvelope
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public string Data { get; set; } = string.Empty;

        [JsonPropertyName("metadata")]
        public Dictionary<string, string>? Metadata { get; set; }
    }

    #endregion
}
