namespace AwsLambda.Host.UnitTests.Core.Features;

[TestSubject(typeof(DefaultResponseFeature<>))]
public class DefaultResponseFeatureTests
{
    #region Test Data Classes

    internal sealed class TestResponse
    {
        public int Id { get; set; }
        public string Message { get; set; } = "";
    }

    #endregion

    #region Constructor Validation Tests

    [Fact]
    internal void Constructor_WithNullSerializer_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new DefaultResponseFeature<string>(null!);
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Constructor_WithValidSerializer_SuccessfullyConstructs(
        DefaultResponseFeature<string> feature
    ) =>
        // Assert
        feature.Should().NotBeNull();

    #endregion

    #region SetResponse and GetResponse Tests

    [Theory]
    [AutoNSubstituteData]
    internal void SetResponse_WithValue_StoresAndRetrievesValue(
        DefaultResponseFeature<string> feature
    )
    {
        // Arrange
        const string expectedResponse = "test-response";

        // Act
        feature.SetResponse(expectedResponse);
        var result = feature.GetResponse();

        // Assert
        result.Should().Be(expectedResponse);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void GetResponse_WithoutSetResponse_ReturnsDefault(
        DefaultResponseFeature<string> feature
    )
    {
        // Act
        var result = feature.GetResponse();

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void GetResponse_WithoutSetResponse_ReturnsDefaultForValueType(
        DefaultResponseFeature<int> feature
    )
    {
        // Act
        var result = feature.GetResponse();

        // Assert
        result.Should().Be(0);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void SetResponse_WithNullValue_StoresAndRetrievesNull(
        DefaultResponseFeature<string?> feature
    )
    {
        // Act
        feature.SetResponse(null);
        var result = feature.GetResponse();

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void SetResponse_MultipleTimesWithDifferentValues_ReturnsLastValue(
        DefaultResponseFeature<string> feature
    )
    {
        // Arrange
        const string firstResponse = "first";
        const string secondResponse = "second";
        const string lastResponse = "last";

        // Act
        feature.SetResponse(firstResponse);
        feature.SetResponse(secondResponse);
        feature.SetResponse(lastResponse);
        var result = feature.GetResponse();

        // Assert
        result.Should().Be(lastResponse);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void SetResponse_WithComplexObject_StoresAndRetrievesObject(
        DefaultResponseFeature<TestResponse> feature
    )
    {
        // Arrange
        var expectedResponse = new TestResponse { Id = 42, Message = "test" };

        // Act
        feature.SetResponse(expectedResponse);
        var result = feature.GetResponse();

        // Assert
        result.Should().BeSameAs(expectedResponse);
    }

    #endregion

    #region SerializeToStream Tests

    [Theory]
    [AutoNSubstituteData]
    internal void SerializeToStream_WhenResponseIsSet_CallsSerializer(
        [Frozen] ILambdaSerializer serializer,
        DefaultResponseFeature<string> feature,
        ILambdaHostContext context
    )
    {
        // Arrange
        const string response = "test-response";
        feature.SetResponse(response);

        // Act
        feature.SerializeToStream(context);

        // Assert
        serializer.Received(1).Serialize(response, context.RawInvocationData.Response);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void SerializeToStream_WhenResponseNotSet_DoesNotCallSerializer(
        [Frozen] ILambdaSerializer serializer,
        DefaultResponseFeature<string> feature,
        ILambdaHostContext context
    )
    {
        // Act
        feature.SerializeToStream(context);

        // Assert
        serializer.DidNotReceive().Serialize(Arg.Any<string>(), Arg.Any<Stream>());
    }

    [Theory]
    [AutoNSubstituteData]
    internal void SerializeToStream_WhenResponseIsSet_ClearsResponseStream(
        DefaultResponseFeature<string> feature,
        ILambdaHostContext context
    )
    {
        // Arrange
        const string response = "test-response";
        feature.SetResponse(response);
        var responseStream = context.RawInvocationData.Response;

        // Act
        feature.SerializeToStream(context);

        // Assert
        responseStream.Received(1).SetLength(0L);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void SerializeToStream_WhenResponseIsSet_ResetsStreamPosition(
        DefaultResponseFeature<string> feature,
        ILambdaHostContext context
    )
    {
        // Arrange
        const string response = "test-response";
        feature.SetResponse(response);
        var responseStream = context.RawInvocationData.Response;

        // Act
        feature.SerializeToStream(context);

        // Assert
        responseStream.Position.Should().Be(0L);
    }

    #endregion

    #region Interface Implementation Tests

    [Theory]
    [AutoNSubstituteData]
    internal void IResponseFeatureGetResponse_DelegatestoGenericGetResponse(
        DefaultResponseFeature<string> feature
    )
    {
        // Arrange
        const string expectedResponse = "interface-test";
        feature.SetResponse(expectedResponse);

        // Act
        var result = ((IResponseFeature)feature).GetResponse();

        // Assert
        result.Should().Be(expectedResponse);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void IResponseFeatureGetResponse_ReturnsObjectType(
        DefaultResponseFeature<string> feature
    )
    {
        // Arrange
        const string response = "test";
        feature.SetResponse(response);

        // Act
        var result = ((IResponseFeature)feature).GetResponse();

        // Assert
        result.Should().BeOfType<string>();
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Feature_ImplementsIResponseFeatureGeneric(
        DefaultResponseFeature<string> feature
    ) =>
        // Assert
        feature.Should().BeAssignableTo<IResponseFeature<string>>();

    [Theory]
    [AutoNSubstituteData]
    internal void Feature_ImplementsIResponseFeatureNonGeneric(
        DefaultResponseFeature<string> feature
    ) =>
        // Assert
        feature.Should().BeAssignableTo<IResponseFeature>();

    #endregion

    #region Generic Type Handling Tests

    [Theory]
    [AutoNSubstituteData]
    internal void Feature_WorksWithReferenceTypes_String(DefaultResponseFeature<string> feature)
    {
        // Arrange
        const string expectedResponse = "reference-type-test";

        // Act
        feature.SetResponse(expectedResponse);
        var result = feature.GetResponse();

        // Assert
        result.Should().Be(expectedResponse);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Feature_WorksWithValueTypes_Int(DefaultResponseFeature<int> feature)
    {
        // Arrange
        const int expectedResponse = 42;

        // Act
        feature.SetResponse(expectedResponse);
        var result = feature.GetResponse();

        // Assert
        result.Should().Be(expectedResponse);
    }

    [Theory]
    [AutoNSubstituteData]
    internal void Feature_WorksWithNullableValueTypes_NullableInt(
        DefaultResponseFeature<int?> feature
    )
    {
        // Arrange
        int? expectedResponse = 99;

        // Act
        feature.SetResponse(expectedResponse);
        var result = feature.GetResponse();

        // Assert
        result.Should().Be(expectedResponse);
    }

    #endregion
}
