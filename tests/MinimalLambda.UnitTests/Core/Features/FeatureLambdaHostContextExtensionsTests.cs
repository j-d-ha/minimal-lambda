namespace MinimalLambda.UnitTests.Core.Features;

[TestSubject(typeof(FeatureLambdaHostContextExtensions))]
public class FeatureLambdaHostContextExtensionsTest
{
    #region GetEvent Tests

    [Theory]
    [AutoNSubstituteData]
    public void GetEvent_ReturnsEventWhenFeatureExistsAndTypeMatches(
        [Frozen] IFeatureCollection features,
        ILambdaHostContext context,
        IEventFeature<TestEvent> eventFeature,
        TestEvent expectedEvent
    )
    {
        // Arrange
        features.Get<IEventFeature>().Returns(eventFeature);
        eventFeature.GetEvent(context).Returns(expectedEvent);

        // Act
        var result = context.GetEvent<TestEvent>();

        // Assert
        result.Should().BeSameAs(expectedEvent);
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetEvent_ReturnsNullWhenFeatureNotInCollection(
        [Frozen] IFeatureCollection features,
        ILambdaHostContext context
    )
    {
        // Arrange
        features.Get<IEventFeature>().Returns((IEventFeature?)null);

        // Act
        var result = context.GetEvent<TestEvent>();

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetEvent_ReturnsNullWhenGetEventReturnsNull(
        [Frozen] IFeatureCollection features,
        ILambdaHostContext context,
        IEventFeature eventFeature
    )
    {
        // Arrange
        features.Get<IEventFeature>().Returns(eventFeature);
        eventFeature.GetEvent(context).Returns((object?)null);

        // Act
        var result = context.GetEvent<TestEvent>();

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetEvent_ReturnsNullWhenEventTypeDoesNotMatch(
        [Frozen] IFeatureCollection features,
        ILambdaHostContext context,
        IEventFeature eventFeature,
        string wrongTypeEvent
    )
    {
        // Arrange
        features.Get<IEventFeature>().Returns(eventFeature);
        eventFeature.GetEvent(context).Returns(wrongTypeEvent);

        // Act
        var result = context.GetEvent<TestEvent>();

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetEvent_WorksWithDifferentEventTypes(
        [Frozen] IFeatureCollection features,
        ILambdaHostContext context,
        IEventFeature<string> eventFeature,
        string stringEvent
    )
    {
        // Arrange
        features.Get<IEventFeature>().Returns(eventFeature);
        eventFeature.GetEvent(context).Returns(stringEvent);

        // Act
        var result = context.GetEvent<string>();

        // Assert
        result.Should().Be(stringEvent);
    }

    #endregion

    #region TryGetEvent Tests

    [Theory]
    [AutoNSubstituteData]
    public void TryGetEvent_ReturnsTrueWhenEventExists(
        [Frozen] IFeatureCollection features,
        ILambdaHostContext context,
        IEventFeature<TestEvent> eventFeature,
        TestEvent expectedEvent
    )
    {
        // Arrange
        features.Get<IEventFeature>().Returns(eventFeature);
        eventFeature.GetEvent(context).Returns(expectedEvent);

        // Act
        var result = context.TryGetEvent(out TestEvent? @event);

        // Assert
        result.Should().BeTrue();
        @event.Should().BeSameAs(expectedEvent);
    }

    [Theory]
    [AutoNSubstituteData]
    public void TryGetEvent_ReturnsFalseWhenFeatureNotFound(
        [Frozen] IFeatureCollection features,
        ILambdaHostContext context
    )
    {
        // Arrange
        features.Get<IEventFeature>().Returns((IEventFeature?)null);

        // Act
        var result = context.TryGetEvent(out TestEvent? @event);

        // Assert
        result.Should().BeFalse();
        @event.Should().BeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    public void TryGetEvent_ReturnsFalseWhenTypeDoesNotMatch(
        [Frozen] IFeatureCollection features,
        ILambdaHostContext context,
        IEventFeature eventFeature,
        string wrongTypeEvent
    )
    {
        // Arrange
        features.Get<IEventFeature>().Returns(eventFeature);
        eventFeature.GetEvent(context).Returns(wrongTypeEvent);

        // Act
        var result = context.TryGetEvent(out TestEvent? @event);

        // Assert
        result.Should().BeFalse();
        @event.Should().BeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    public void TryGetEvent_WorksWithDifferentEventTypes(
        [Frozen] IFeatureCollection features,
        ILambdaHostContext context,
        IEventFeature<string> eventFeature,
        string stringEvent
    )
    {
        // Arrange
        features.Get<IEventFeature>().Returns(eventFeature);
        eventFeature.GetEvent(context).Returns(stringEvent);

        // Act
        var result = context.TryGetEvent(out string? @event);

        // Assert
        result.Should().BeTrue();
        @event.Should().Be(stringEvent);
    }

    #endregion

    #region GetResponse Tests

    [Theory]
    [AutoNSubstituteData]
    public void GetResponse_ReturnsResponseWhenFeatureExistsAndTypeMatches(
        [Frozen] IFeatureCollection features,
        ILambdaHostContext context,
        IResponseFeature<TestResponse> responseFeature,
        TestResponse expectedResponse
    )
    {
        // Arrange
        features.Get<IResponseFeature>().Returns(responseFeature);
        responseFeature.GetResponse().Returns(expectedResponse);

        // Act
        var result = context.GetResponse<TestResponse>();

        // Assert
        result.Should().BeSameAs(expectedResponse);
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetResponse_ReturnsNullWhenFeatureNotInCollection(
        [Frozen] IFeatureCollection features,
        ILambdaHostContext context
    )
    {
        // Arrange
        features.Get<IResponseFeature>().Returns((IResponseFeature?)null);

        // Act
        var result = context.GetResponse<TestResponse>();

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetResponse_ReturnsNullWhenGetResponseReturnsNull(
        [Frozen] IFeatureCollection features,
        ILambdaHostContext context,
        IResponseFeature responseFeature
    )
    {
        // Arrange
        features.Get<IResponseFeature>().Returns(responseFeature);
        responseFeature.GetResponse().Returns((object?)null);

        // Act
        var result = context.GetResponse<TestResponse>();

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetResponse_ReturnsNullWhenResponseTypeDoesNotMatch(
        [Frozen] IFeatureCollection features,
        ILambdaHostContext context,
        IResponseFeature responseFeature,
        string wrongTypeResponse
    )
    {
        // Arrange
        features.Get<IResponseFeature>().Returns(responseFeature);
        responseFeature.GetResponse().Returns(wrongTypeResponse);

        // Act
        var result = context.GetResponse<TestResponse>();

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetResponse_WorksWithDifferentResponseTypes(
        [Frozen] IFeatureCollection features,
        ILambdaHostContext context,
        IResponseFeature<string> responseFeature,
        string stringResponse
    )
    {
        // Arrange
        features.Get<IResponseFeature>().Returns(responseFeature);
        responseFeature.GetResponse().Returns(stringResponse);

        // Act
        var result = context.GetResponse<string>();

        // Assert
        result.Should().Be(stringResponse);
    }

    #endregion

    #region TryGetResponse Tests

    [Theory]
    [AutoNSubstituteData]
    public void TryGetResponse_ReturnsTrueWhenResponseExists(
        [Frozen] IFeatureCollection features,
        ILambdaHostContext context,
        IResponseFeature<TestResponse> responseFeature,
        TestResponse expectedResponse
    )
    {
        // Arrange
        features.Get<IResponseFeature>().Returns(responseFeature);
        responseFeature.GetResponse().Returns(expectedResponse);

        // Act
        var result = context.TryGetResponse(out TestResponse? response);

        // Assert
        result.Should().BeTrue();
        response.Should().BeSameAs(expectedResponse);
    }

    [Theory]
    [AutoNSubstituteData]
    public void TryGetResponse_ReturnsFalseWhenFeatureNotFound(
        [Frozen] IFeatureCollection features,
        ILambdaHostContext context
    )
    {
        // Arrange
        features.Get<IResponseFeature>().Returns((IResponseFeature?)null);

        // Act
        var result = context.TryGetResponse(out TestResponse? response);

        // Assert
        result.Should().BeFalse();
        response.Should().BeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    public void TryGetResponse_ReturnsFalseWhenTypeDoesNotMatch(
        [Frozen] IFeatureCollection features,
        ILambdaHostContext context,
        IResponseFeature responseFeature,
        string wrongTypeResponse
    )
    {
        // Arrange
        features.Get<IResponseFeature>().Returns(responseFeature);
        responseFeature.GetResponse().Returns(wrongTypeResponse);

        // Act
        var result = context.TryGetResponse(out TestResponse? response);

        // Assert
        result.Should().BeFalse();
        response.Should().BeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    public void TryGetResponse_WorksWithDifferentResponseTypes(
        [Frozen] IFeatureCollection features,
        ILambdaHostContext context,
        IResponseFeature<string> responseFeature,
        string stringResponse
    )
    {
        // Arrange
        features.Get<IResponseFeature>().Returns(responseFeature);
        responseFeature.GetResponse().Returns(stringResponse);

        // Act
        var result = context.TryGetResponse(out string? response);

        // Assert
        result.Should().BeTrue();
        response.Should().Be(stringResponse);
    }

    #endregion

    #region GetRequiredEvent Tests

    [Theory]
    [AutoNSubstituteData]
    public void GetRequiredEvent_ReturnsEventWhenFeatureExistsAndTypeMatches(
        [Frozen] IFeatureCollection features,
        ILambdaHostContext context,
        IEventFeature<TestEvent> eventFeature,
        TestEvent expectedEvent
    )
    {
        // Arrange
        features.Get<IEventFeature>().Returns(eventFeature);
        eventFeature.GetEvent(context).Returns(expectedEvent);

        // Act
        var result = context.GetRequiredEvent<TestEvent>();

        // Assert
        result.Should().BeSameAs(expectedEvent);
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetRequiredEvent_ThrowsInvalidOperationExceptionWhenFeatureNotFound(
        [Frozen] IFeatureCollection features,
        ILambdaHostContext context
    )
    {
        // Arrange
        features.Get<IEventFeature>().Returns((IEventFeature?)null);

        // Act & Assert
        var act = () => context.GetRequiredEvent<TestEvent>();
        act.Should()
            .ThrowExactly<InvalidOperationException>()
            .WithMessage(
                $"Lambda event of type '{typeof(TestEvent).FullName}' is not available in the context."
            );
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetRequiredEvent_ThrowsInvalidOperationExceptionWhenEventIsNull(
        [Frozen] IFeatureCollection features,
        ILambdaHostContext context,
        IEventFeature eventFeature
    )
    {
        // Arrange
        features.Get<IEventFeature>().Returns(eventFeature);
        eventFeature.GetEvent(context).Returns((object?)null);

        // Act & Assert
        var act = () => context.GetRequiredEvent<TestEvent>();
        act.Should()
            .ThrowExactly<InvalidOperationException>()
            .WithMessage(
                $"Lambda event of type '{typeof(TestEvent).FullName}' is not available in the context."
            );
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetRequiredEvent_ThrowsInvalidOperationExceptionWhenTypeDoesNotMatch(
        [Frozen] IFeatureCollection features,
        ILambdaHostContext context,
        IEventFeature eventFeature,
        string wrongTypeEvent
    )
    {
        // Arrange
        features.Get<IEventFeature>().Returns(eventFeature);
        eventFeature.GetEvent(context).Returns(wrongTypeEvent);

        // Act & Assert
        var act = () => context.GetRequiredEvent<TestEvent>();
        act.Should()
            .ThrowExactly<InvalidOperationException>()
            .WithMessage(
                $"Lambda event of type '{typeof(TestEvent).FullName}' is not available in the context."
            );
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetRequiredEvent_WorksWithDifferentEventTypes(
        [Frozen] IFeatureCollection features,
        ILambdaHostContext context,
        IEventFeature<string> eventFeature,
        string stringEvent
    )
    {
        // Arrange
        features.Get<IEventFeature>().Returns(eventFeature);
        eventFeature.GetEvent(context).Returns(stringEvent);

        // Act
        var result = context.GetRequiredEvent<string>();

        // Assert
        result.Should().Be(stringEvent);
    }

    #endregion

    #region GetRequiredResponse Tests

    [Theory]
    [AutoNSubstituteData]
    public void GetRequiredResponse_ReturnsResponseWhenFeatureExistsAndTypeMatches(
        [Frozen] IFeatureCollection features,
        ILambdaHostContext context,
        IResponseFeature<TestResponse> responseFeature,
        TestResponse expectedResponse
    )
    {
        // Arrange
        features.Get<IResponseFeature>().Returns(responseFeature);
        responseFeature.GetResponse().Returns(expectedResponse);

        // Act
        var result = context.GetRequiredResponse<TestResponse>();

        // Assert
        result.Should().BeSameAs(expectedResponse);
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetRequiredResponse_ThrowsInvalidOperationExceptionWhenFeatureNotFound(
        [Frozen] IFeatureCollection features,
        ILambdaHostContext context
    )
    {
        // Arrange
        features.Get<IResponseFeature>().Returns((IResponseFeature?)null);

        // Act & Assert
        var act = () => context.GetRequiredResponse<TestResponse>();
        act.Should()
            .ThrowExactly<InvalidOperationException>()
            .WithMessage(
                $"Lambda response of type '{typeof(TestResponse).FullName}' is not available in the context."
            );
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetRequiredResponse_ThrowsInvalidOperationExceptionWhenResponseIsNull(
        [Frozen] IFeatureCollection features,
        ILambdaHostContext context,
        IResponseFeature responseFeature
    )
    {
        // Arrange
        features.Get<IResponseFeature>().Returns(responseFeature);
        responseFeature.GetResponse().Returns((object?)null);

        // Act & Assert
        var act = () => context.GetRequiredResponse<TestResponse>();
        act.Should()
            .ThrowExactly<InvalidOperationException>()
            .WithMessage(
                $"Lambda response of type '{typeof(TestResponse).FullName}' is not available in the context."
            );
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetRequiredResponse_ThrowsInvalidOperationExceptionWhenTypeDoesNotMatch(
        [Frozen] IFeatureCollection features,
        ILambdaHostContext context,
        IResponseFeature responseFeature,
        string wrongTypeResponse
    )
    {
        // Arrange
        features.Get<IResponseFeature>().Returns(responseFeature);
        responseFeature.GetResponse().Returns(wrongTypeResponse);

        // Act & Assert
        var act = () => context.GetRequiredResponse<TestResponse>();
        act.Should()
            .ThrowExactly<InvalidOperationException>()
            .WithMessage(
                $"Lambda response of type '{typeof(TestResponse).FullName}' is not available in the context."
            );
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetRequiredResponse_WorksWithDifferentResponseTypes(
        [Frozen] IFeatureCollection features,
        ILambdaHostContext context,
        IResponseFeature<string> responseFeature,
        string stringResponse
    )
    {
        // Arrange
        features.Get<IResponseFeature>().Returns(responseFeature);
        responseFeature.GetResponse().Returns(stringResponse);

        // Act
        var result = context.GetRequiredResponse<string>();

        // Assert
        result.Should().Be(stringResponse);
    }

    #endregion

    #region Null Context Tests

    [Fact]
    public void GetEvent_ThrowsArgumentNullExceptionWhenContextIsNull()
    {
        // Act & Assert
        var act = () => ((ILambdaHostContext?)null)!.GetEvent<TestEvent>();
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void TryGetEvent_ThrowsArgumentNullExceptionWhenContextIsNull()
    {
        // Act & Assert
        var act = () => ((ILambdaHostContext?)null)!.TryGetEvent(out TestEvent? _);
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void GetResponse_ThrowsArgumentNullExceptionWhenContextIsNull()
    {
        // Act & Assert
        var act = () => ((ILambdaHostContext?)null)!.GetResponse<TestResponse>();
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void TryGetResponse_ThrowsArgumentNullExceptionWhenContextIsNull()
    {
        // Act & Assert
        var act = () => ((ILambdaHostContext?)null)!.TryGetResponse(out TestResponse? _);
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void GetRequiredEvent_ThrowsArgumentNullExceptionWhenContextIsNull()
    {
        // Act & Assert
        var act = () => ((ILambdaHostContext?)null)!.GetRequiredEvent<TestEvent>();
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void GetRequiredResponse_ThrowsArgumentNullExceptionWhenContextIsNull()
    {
        // Act & Assert
        var act = () => ((ILambdaHostContext?)null)!.GetRequiredResponse<TestResponse>();
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    #endregion

    #region Test Fixtures

    public class TestEvent
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public class TestResponse
    {
        public string? Message { get; set; }
        public int Status { get; set; }
    }

    #endregion
}
