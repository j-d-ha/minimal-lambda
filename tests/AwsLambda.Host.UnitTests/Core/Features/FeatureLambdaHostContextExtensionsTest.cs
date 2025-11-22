using AwesomeAssertions;
using AwsLambda.Host.Core;
using JetBrains.Annotations;
using NSubstitute;
using Xunit;

namespace AwsLambda.Host.UnitTests.Core.Features;

[TestSubject(typeof(FeatureLambdaHostContextExtensions))]
public class FeatureLambdaHostContextExtensionsTest
{
    #region GetEvent Tests

    [Theory]
    [AutoNSubstituteData]
    public void GetEvent_ReturnsEventWhenFeatureExistsAndTypeMatches(
        ILambdaHostContext context,
        IEventFeature eventFeature,
        IFeatureCollection features
    )
    {
        // Arrange
        var expectedEvent = new TestEvent { Id = 1, Name = "test" };

        features.Get<IEventFeature>().Returns(eventFeature);
        eventFeature.GetEvent(context).Returns(expectedEvent);
        context.Features.Returns(features);

        // Act
        var result = context.GetEvent<TestEvent>();

        // Assert
        result.Should().BeSameAs(expectedEvent);
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetEvent_ReturnsNullWhenFeatureNotInCollection(
        ILambdaHostContext context,
        IFeatureCollection features
    )
    {
        // Arrange
        features.Get<IEventFeature>().Returns((IEventFeature?)null);
        context.Features.Returns(features);

        // Act
        var result = context.GetEvent<TestEvent>();

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetEvent_ReturnsNullWhenGetEventReturnsNull(
        ILambdaHostContext context,
        IEventFeature eventFeature,
        IFeatureCollection features
    )
    {
        // Arrange
        features.Get<IEventFeature>().Returns(eventFeature);
        eventFeature.GetEvent(context).Returns((object?)null);
        context.Features.Returns(features);

        // Act
        var result = context.GetEvent<TestEvent>();

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetEvent_ReturnsNullWhenEventTypeDoesNotMatch(
        ILambdaHostContext context,
        IEventFeature eventFeature,
        IFeatureCollection features
    )
    {
        // Arrange
        var wrongTypeEvent = "not-a-test-event";

        features.Get<IEventFeature>().Returns(eventFeature);
        eventFeature.GetEvent(context).Returns(wrongTypeEvent);
        context.Features.Returns(features);

        // Act
        var result = context.GetEvent<TestEvent>();

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetEvent_WorksWithDifferentEventTypes(
        ILambdaHostContext context,
        IEventFeature eventFeature,
        IFeatureCollection features
    )
    {
        // Arrange
        var stringEvent = "test-event-string";

        features.Get<IEventFeature>().Returns(eventFeature);
        eventFeature.GetEvent(context).Returns(stringEvent);
        context.Features.Returns(features);

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
        ILambdaHostContext context,
        IEventFeature eventFeature,
        IFeatureCollection features
    )
    {
        // Arrange
        var expectedEvent = new TestEvent { Id = 1, Name = "test" };

        features.Get<IEventFeature>().Returns(eventFeature);
        eventFeature.GetEvent(context).Returns(expectedEvent);
        context.Features.Returns(features);

        // Act
        var result = context.TryGetEvent(out TestEvent? @event);

        // Assert
        result.Should().BeTrue();
        @event.Should().BeSameAs(expectedEvent);
    }

    [Theory]
    [AutoNSubstituteData]
    public void TryGetEvent_ReturnsFalseWhenFeatureNotFound(
        ILambdaHostContext context,
        IFeatureCollection features
    )
    {
        // Arrange
        features.Get<IEventFeature>().Returns((IEventFeature?)null);
        context.Features.Returns(features);

        // Act
        var result = context.TryGetEvent(out TestEvent? @event);

        // Assert
        result.Should().BeFalse();
        @event.Should().BeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    public void TryGetEvent_ReturnsFalseWhenTypeDoesNotMatch(
        ILambdaHostContext context,
        IEventFeature eventFeature,
        IFeatureCollection features
    )
    {
        // Arrange
        var wrongTypeEvent = "not-a-test-event";

        features.Get<IEventFeature>().Returns(eventFeature);
        eventFeature.GetEvent(context).Returns(wrongTypeEvent);
        context.Features.Returns(features);

        // Act
        var result = context.TryGetEvent(out TestEvent? @event);

        // Assert
        result.Should().BeFalse();
        @event.Should().BeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    public void TryGetEvent_WorksWithDifferentEventTypes(
        ILambdaHostContext context,
        IEventFeature eventFeature,
        IFeatureCollection features
    )
    {
        // Arrange
        var stringEvent = "test-event";

        features.Get<IEventFeature>().Returns(eventFeature);
        eventFeature.GetEvent(context).Returns(stringEvent);
        context.Features.Returns(features);

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
        ILambdaHostContext context,
        IResponseFeature responseFeature,
        IFeatureCollection features
    )
    {
        // Arrange
        var expectedResponse = new TestResponse { Status = 200, Message = "OK" };

        features.Get<IResponseFeature>().Returns(responseFeature);
        responseFeature.GetResponse().Returns(expectedResponse);
        context.Features.Returns(features);

        // Act
        var result = context.GetResponse<TestResponse>();

        // Assert
        result.Should().BeSameAs(expectedResponse);
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetResponse_ReturnsNullWhenFeatureNotInCollection(
        ILambdaHostContext context,
        IFeatureCollection features
    )
    {
        // Arrange
        features.Get<IResponseFeature>().Returns((IResponseFeature?)null);
        context.Features.Returns(features);

        // Act
        var result = context.GetResponse<TestResponse>();

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetResponse_ReturnsNullWhenGetResponseReturnsNull(
        ILambdaHostContext context,
        IResponseFeature responseFeature,
        IFeatureCollection features
    )
    {
        // Arrange
        features.Get<IResponseFeature>().Returns(responseFeature);
        responseFeature.GetResponse().Returns((object?)null);
        context.Features.Returns(features);

        // Act
        var result = context.GetResponse<TestResponse>();

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetResponse_ReturnsNullWhenResponseTypeDoesNotMatch(
        ILambdaHostContext context,
        IResponseFeature responseFeature,
        IFeatureCollection features
    )
    {
        // Arrange
        var wrongTypeResponse = "not-a-test-response";

        features.Get<IResponseFeature>().Returns(responseFeature);
        responseFeature.GetResponse().Returns(wrongTypeResponse);
        context.Features.Returns(features);

        // Act
        var result = context.GetResponse<TestResponse>();

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetResponse_WorksWithDifferentResponseTypes(
        ILambdaHostContext context,
        IResponseFeature responseFeature,
        IFeatureCollection features
    )
    {
        // Arrange
        var stringResponse = "test-response-string";

        features.Get<IResponseFeature>().Returns(responseFeature);
        responseFeature.GetResponse().Returns(stringResponse);
        context.Features.Returns(features);

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
        ILambdaHostContext context,
        IResponseFeature responseFeature,
        IFeatureCollection features
    )
    {
        // Arrange
        var expectedResponse = new TestResponse { Status = 200, Message = "OK" };

        features.Get<IResponseFeature>().Returns(responseFeature);
        responseFeature.GetResponse().Returns(expectedResponse);
        context.Features.Returns(features);

        // Act
        var result = context.TryGetResponse(out TestResponse? response);

        // Assert
        result.Should().BeTrue();
        response.Should().BeSameAs(expectedResponse);
    }

    [Theory]
    [AutoNSubstituteData]
    public void TryGetResponse_ReturnsFalseWhenFeatureNotFound(
        ILambdaHostContext context,
        IFeatureCollection features
    )
    {
        // Arrange
        features.Get<IResponseFeature>().Returns((IResponseFeature?)null);
        context.Features.Returns(features);

        // Act
        var result = context.TryGetResponse(out TestResponse? response);

        // Assert
        result.Should().BeFalse();
        response.Should().BeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    public void TryGetResponse_ReturnsFalseWhenTypeDoesNotMatch(
        ILambdaHostContext context,
        IResponseFeature responseFeature,
        IFeatureCollection features
    )
    {
        // Arrange
        var wrongTypeResponse = "not-a-test-response";

        features.Get<IResponseFeature>().Returns(responseFeature);
        responseFeature.GetResponse().Returns(wrongTypeResponse);
        context.Features.Returns(features);

        // Act
        var result = context.TryGetResponse(out TestResponse? response);

        // Assert
        result.Should().BeFalse();
        response.Should().BeNull();
    }

    [Theory]
    [AutoNSubstituteData]
    public void TryGetResponse_WorksWithDifferentResponseTypes(
        ILambdaHostContext context,
        IResponseFeature responseFeature,
        IFeatureCollection features
    )
    {
        // Arrange
        var stringResponse = "test-response";

        features.Get<IResponseFeature>().Returns(responseFeature);
        responseFeature.GetResponse().Returns(stringResponse);
        context.Features.Returns(features);

        // Act
        var result = context.TryGetResponse(out string? response);

        // Assert
        result.Should().BeTrue();
        response.Should().Be(stringResponse);
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

    #endregion

    #region Test Fixtures

    private class TestEvent
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    private class TestResponse
    {
        public string? Message { get; set; }
        public int Status { get; set; }
    }

    #endregion
}
