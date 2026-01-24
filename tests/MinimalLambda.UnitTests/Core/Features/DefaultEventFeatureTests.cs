using NSubstitute.ExceptionExtensions;

namespace MinimalLambda.UnitTests.Core.Features;

[TestSubject(typeof(DefaultEventFeature<>))]
public class DefaultEventFeatureTests
{
    #region RawInvocationData Integration Tests

    [Theory]
    [AutoNSubstituteData]
    internal void GetEvent_PassesRawEventStreamToSerializer(
        [Frozen] ILambdaSerializer serializer,
        [Frozen] IFeatureCollection features,
        [Frozen] IInvocationDataFeature dataFeature,
        [Frozen] Stream stream,
        DefaultEventFeature<string> feature,
        ILambdaInvocationContext context)
    {
        // Arrange
        features.Get<IInvocationDataFeature>().Returns(dataFeature);
        dataFeature.EventStream.Returns(stream);
        serializer.Deserialize<string>(Arg.Any<Stream>()).Returns("result");

        // Act
        _ = feature.GetEvent(context);

        // Assert
        serializer.Received(1).Deserialize<string>(stream);
    }

    #endregion

    #region Constructor Validation Tests

    [Fact]
    internal void Constructor_WithNullSerializer_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new DefaultEventFeature<string>();
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Constructor_WithValidSerializer_SuccessfullyConstructs(
        ILambdaSerializer serializer)
    {
        // Act
        var feature = new DefaultEventFeature<string>(serializer);

        // Assert
        feature.Should().NotBeNull();
    }

    #endregion

    #region GetEvent Basic Functionality Tests

    [Theory]
    [AutoNSubstituteData]
    internal void GetEvent_WithSimpleStringEvent_ReturnsDeserializedString(
        [Frozen] ILambdaSerializer serializer,
        DefaultEventFeature<string> feature,
        ILambdaInvocationContext context)
    {
        // Arrange
        const string expectedEvent = "test-event-data";
        serializer.Deserialize<string>(Arg.Any<Stream>()).Returns(expectedEvent);

        // Act
        var result = feature.GetEvent(context);

        // Assert
        result.Should().Be(expectedEvent);
        serializer.Received(1).Deserialize<string>(Arg.Any<Stream>());
    }

    [Theory]
    [AutoNSubstituteData]
    internal void GetEvent_WithComplexObject_ReturnsDeserializedObject(
        [Frozen] ILambdaSerializer serializer,
        DefaultEventFeature<TestEvent> feature,
        ILambdaInvocationContext context)
    {
        // Arrange
        var expectedEvent = new TestEvent { Id = 42, Name = "test" };
        serializer.Deserialize<TestEvent>(Arg.Any<Stream>()).Returns(expectedEvent);

        // Act
        var result = feature.GetEvent(context);

        // Assert
        result.Should().BeEquivalentTo(expectedEvent);
        result.Should().BeSameAs(expectedEvent);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void GetEvent_WithNullableEvent_ReturnsDeserializedNullValue(
        [Frozen] ILambdaSerializer serializer,
        DefaultEventFeature<string?> feature,
        ILambdaInvocationContext context)
    {
        // Arrange
        string? expectedEvent = null;
        serializer.Deserialize<string?>(Arg.Any<Stream>()).Returns(expectedEvent);

        // Act
        var result = feature.GetEvent(context);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Lazy Deserialization Tests

    [Theory]
    [AutoNSubstituteData]
    internal void GetEvent_CallsDeserializerOnFirstInvocation(
        [Frozen] ILambdaSerializer serializer,
        DefaultEventFeature<string> feature,
        ILambdaInvocationContext context)
    {
        // Arrange
        const string expectedEvent = "test-data";
        serializer.Deserialize<string>(Arg.Any<Stream>()).Returns(expectedEvent);

        // Act
        _ = feature.GetEvent(context);

        // Assert
        serializer.Received(1).Deserialize<string>(Arg.Any<Stream>());
    }

    [Theory]
    [AutoNSubstituteData]
    internal void GetEvent_CachesResultAndDoesNotDeserializeOnSecondInvocation(
        [Frozen] ILambdaSerializer serializer,
        DefaultEventFeature<string> feature,
        ILambdaInvocationContext context)
    {
        // Arrange
        const string expectedEvent = "test-data";
        serializer.Deserialize<string>(Arg.Any<Stream>()).Returns(expectedEvent);

        // Act
        var result1 = feature.GetEvent(context);
        var result2 = feature.GetEvent(context);

        // Assert
        result1.Should().Be(expectedEvent);
        result2.Should().Be(expectedEvent);
        serializer.Received(1).Deserialize<string>(Arg.Any<Stream>());
    }

    [Theory]
    [AutoNSubstituteData]
    internal void GetEvent_ReturnsSameCachedInstanceOnMultipleInvocations(
        [Frozen] ILambdaSerializer serializer,
        DefaultEventFeature<TestEvent> feature,
        ILambdaInvocationContext context)
    {
        // Arrange
        var expectedEvent = new TestEvent { Id = 1, Name = "cached" };
        serializer.Deserialize<TestEvent>(Arg.Any<Stream>()).Returns(expectedEvent);

        // Act
        var result1 = feature.GetEvent(context);
        var result2 = feature.GetEvent(context);
        var result3 = feature.GetEvent(context);

        // Assert
        result1.Should().BeSameAs(result2);
        result2.Should().BeSameAs(result3);
        serializer.Received(1).Deserialize<TestEvent>(Arg.Any<Stream>());
    }

    [Theory]
    [AutoNSubstituteData]
    internal void GetEvent_WithMultipleInstances_CachesPerInstance(
        ILambdaSerializer serializer1,
        ILambdaInvocationContext context1,
        ILambdaSerializer serializer2,
        ILambdaInvocationContext context2)
    {
        // Arrange
        const string event1 = "event-1";
        const string event2 = "event-2";

        serializer1.Deserialize<string>(Arg.Any<Stream>()).Returns(event1);
        serializer2.Deserialize<string>(Arg.Any<Stream>()).Returns(event2);

        var feature1 = new DefaultEventFeature<string>(serializer1);
        var feature2 = new DefaultEventFeature<string>(serializer2);

        // Act
        var result1 = feature1.GetEvent(context1);
        var result2 = feature2.GetEvent(context2);

        // Assert
        result1.Should().Be(event1);
        result2.Should().Be(event2);
        serializer1.Received(1).Deserialize<string>(Arg.Any<Stream>());
        serializer2.Received(1).Deserialize<string>(Arg.Any<Stream>());
    }

    #endregion

    #region Generic Type Handling Tests

    [Theory]
    [AutoNSubstituteData]
    internal void GetEvent_WithGenericListType_ReturnsDeserializedList(
        [Frozen] ILambdaSerializer serializer,
        DefaultEventFeature<List<TestEvent>> feature,
        ILambdaInvocationContext context)
    {
        // Arrange
        var expectedList = new List<TestEvent>
        {
            new() { Id = 1, Name = "item1" }, new() { Id = 2, Name = "item2" },
        };
        serializer.Deserialize<List<TestEvent>>(Arg.Any<Stream>()).Returns(expectedList);

        // Act
        var result = feature.GetEvent(context);

        // Assert
        result.Should().BeEquivalentTo(expectedList);
        result.Count.Should().Be(2);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void GetEvent_WithGenericDictionaryType_ReturnsDeserializedDictionary(
        [Frozen] ILambdaSerializer serializer,
        DefaultEventFeature<Dictionary<string, int>> feature,
        ILambdaInvocationContext context)
    {
        // Arrange
        var expectedDict = new Dictionary<string, int> { { "key1", 10 }, { "key2", 20 } };
        serializer.Deserialize<Dictionary<string, int>>(Arg.Any<Stream>()).Returns(expectedDict);

        // Act
        var result = feature.GetEvent(context);

        // Assert
        result.Should().BeEquivalentTo(expectedDict);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void GetEvent_WithNestedGenericType_ReturnsDeserializedNestedType(
        [Frozen] ILambdaSerializer serializer,
        DefaultEventFeature<NestedEvent> feature,
        ILambdaInvocationContext context)
    {
        // Arrange
        var expectedNested = new NestedEvent
        {
            Items = new List<TestEvent> { new() { Id = 1, Name = "nested1" } },
        };
        serializer.Deserialize<NestedEvent>(Arg.Any<Stream>()).Returns(expectedNested);

        // Act
        var result = feature.GetEvent(context);

        // Assert
        result.Should().BeEquivalentTo(expectedNested);
    }

    #endregion

    #region Explicit Interface Implementation Tests

    [Theory]
    [AutoNSubstituteData]
    internal void IEventFeatureGetEvent_ReturnsObjectCastFromGenericGetEvent(
        [Frozen] ILambdaSerializer serializer,
        DefaultEventFeature<string> feature,
        ILambdaInvocationContext context)
    {
        // Arrange
        const string expectedEvent = "interface-test";
        serializer.Deserialize<string>(Arg.Any<Stream>()).Returns(expectedEvent);

        // Act
        var result = ((IEventFeature)feature).GetEvent(context);

        // Assert
        result.Should().Be(expectedEvent);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void IEventFeatureGetEvent_AndGenericGetEvent_ReturnSameCachedInstance(
        [Frozen] ILambdaSerializer serializer,
        DefaultEventFeature<TestEvent> feature,
        ILambdaInvocationContext context)
    {
        // Arrange
        var expectedEvent = new TestEvent { Id = 99, Name = "same" };
        serializer.Deserialize<TestEvent>(Arg.Any<Stream>()).Returns(expectedEvent);

        // Act
        var genericResult = feature.GetEvent(context);
        var interfaceResult = ((IEventFeature)feature).GetEvent(context);

        // Assert
        genericResult.Should().BeSameAs(interfaceResult);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void GenericAndNonGenericInterfaces_AreImplementedCorrectly(
        [Frozen] ILambdaSerializer serializer)
    {
        // Arrange
        var feature = new DefaultEventFeature<string>(serializer);

        // Assert
        feature.Should().BeAssignableTo<IEventFeature<string>>();
        feature.Should().BeAssignableTo<IEventFeature>();
    }

    #endregion

    #region Deserialization Exception Tests

    [Theory]
    [AutoNSubstituteData]
    internal void GetEvent_WhenDeserializerThrowsException_PropagatesException(
        [Frozen] ILambdaSerializer serializer,
        DefaultEventFeature<string> feature,
        ILambdaInvocationContext context)
    {
        // Arrange
        serializer
            .Deserialize<string>(Arg.Any<Stream>())
            .Throws(new InvalidOperationException("Deserialization failed"));

        // Act & Assert
        var act = () => feature.GetEvent(context);
        act.Should().ThrowExactly<InvalidOperationException>();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void GetEvent_AfterDeserializationException_RethrowsExceptionOnNextCall(
        [Frozen] ILambdaSerializer serializer,
        DefaultEventFeature<string> feature,
        ILambdaInvocationContext context)
    {
        // Arrange
        serializer
            .Deserialize<string>(Arg.Any<Stream>())
            .Throws(new InvalidOperationException("Error"));

        // Act & Assert
        var act1 = () => feature.GetEvent(context);
        act1.Should().ThrowExactly<InvalidOperationException>();

        var act2 = () => feature.GetEvent(context);
        act2.Should().ThrowExactly<InvalidOperationException>();

        serializer.Received(2).Deserialize<string>(Arg.Any<Stream>());
    }

    #endregion

    #region Edge Cases Tests

    [Theory]
    [AutoNSubstituteData]
    internal void GetEvent_WithEmptyMemoryStream_DeserializesEmptyStream(
        [Frozen] ILambdaSerializer serializer,
        DefaultEventFeature<string> feature,
        ILambdaInvocationContext context)
    {
        // Arrange
        serializer.Deserialize<string>(Arg.Any<Stream>()).Returns("");

        // Act
        var result = feature.GetEvent(context);

        // Assert
        result.Should().Be("");
    }

    [Theory]
    [AutoNSubstituteData]
    internal void GetEvent_WithValueTypeDefaultValue_ReturnsDefaultOfT(
        [Frozen] ILambdaSerializer serializer,
        DefaultEventFeature<int> feature,
        ILambdaInvocationContext context)
    {
        // Arrange
        serializer.Deserialize<int>(Arg.Any<Stream>()).Returns(0);

        // Act
        var result = feature.GetEvent(context);

        // Assert
        result.Should().Be(0);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void GetEvent_WithLargeObject_ReturnsDeserializedLargeObject(
        [Frozen] ILambdaSerializer serializer,
        DefaultEventFeature<TestEvent> feature,
        ILambdaInvocationContext context)
    {
        // Arrange
        var largeEvent = new TestEvent { Id = int.MaxValue, Name = new string('x', 10000) };
        serializer.Deserialize<TestEvent>(Arg.Any<Stream>()).Returns(largeEvent);

        // Act
        var result = feature.GetEvent(context);

        // Assert
        result.Should().BeEquivalentTo(largeEvent);
        result.Name.Should().HaveLength(10000);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void GetEvent_WithNullableStruct_ReturnsDeserializedNullValue(
        [Frozen] ILambdaSerializer serializer,
        DefaultEventFeature<int?> feature,
        ILambdaInvocationContext context)
    {
        // Arrange
        int? expectedEvent = null;
        serializer.Deserialize<int?>(Arg.Any<Stream>()).Returns(expectedEvent);

        // Act
        var result = feature.GetEvent(context);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void GetEvent_WithNullableStructValue_ReturnsDeserializedValue(
        [Frozen] ILambdaSerializer serializer,
        DefaultEventFeature<int?> feature,
        ILambdaInvocationContext context)
    {
        // Arrange
        int? expectedEvent = 42;
        serializer.Deserialize<int?>(Arg.Any<Stream>()).Returns(expectedEvent);

        // Act
        var result = feature.GetEvent(context);

        // Assert
        result.Should().Be(42);
    }

    #endregion

    #region Test Data Classes

    internal sealed class TestEvent
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    internal sealed class NestedEvent
    {
        public List<TestEvent> Items { get; set; } = [];
    }

    #endregion
}
