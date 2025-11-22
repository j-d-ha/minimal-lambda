using AwesomeAssertions;
using AwsLambda.Host.Core;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using OpenTelemetry.Trace;
using Xunit;

namespace AwsLambda.Host.OpenTelemetry.UnitTests;

[TestSubject(typeof(LambdaOpenTelemetryServiceProviderExtensions))]
public class LambdaOpenTelemetryServiceProviderExtensionsTests
{
    private readonly IServiceProvider _serviceProvider = Substitute.For<IServiceProvider>();
    private readonly TracerProvider _tracerProvider = Substitute.For<TracerProvider>();

    public LambdaOpenTelemetryServiceProviderExtensionsTests() =>
        _serviceProvider.GetService(typeof(TracerProvider)).Returns(_tracerProvider);

    #region GetOpenTelemetryTracer<TEvent, TResponse>

    [Fact]
    public void GetOpenTelemetryTracer_WithNullServiceProvider_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceProvider? nullProvider = null;

        // Act
        var action = () => nullProvider!.GetOpenTelemetryTracer<TestEvent, TestResponse>();

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void GetOpenTelemetryTracer_WithoutTracerProvider_ThrowsInvalidOperationException()
    {
        // Arrange
        _serviceProvider.GetService(typeof(TracerProvider)).Returns(null);

        // Act
        var action = () => _serviceProvider.GetOpenTelemetryTracer<TestEvent, TestResponse>();

        // Assert
        action.Should().ThrowExactly<InvalidOperationException>();
    }

    [Fact]
    public void GetOpenTelemetryTracer_ReturnsMiddlewareFunction()
    {
        // Act
        var middleware = _serviceProvider.GetOpenTelemetryTracer<TestEvent, TestResponse>();

        // Assert
        middleware.Should().NotBeNull();
        middleware.Should().BeOfType<Func<LambdaInvocationDelegate, LambdaInvocationDelegate>>();
    }

    [Fact]
    public async Task GetOpenTelemetryTracer_WithIncorrectEventType_ThrowsInvalidOperationException()
    {
        // Arrange
        var middleware = _serviceProvider.GetOpenTelemetryTracer<TestEvent, TestResponse>();
        var nextDelegate = Substitute.For<LambdaInvocationDelegate>();
        var wrappedDelegate = middleware(nextDelegate);

        var mocks = CreateMocks();

        mocks.EventFeature.GetEvent(mocks.Context).Returns(new object());

        // Act
        var action = async () => await wrappedDelegate(mocks.Context);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetOpenTelemetryTracer_WithIncorrectResponseType_ThrowsInvalidOperationException()
    {
        // Arrange
        var middleware = _serviceProvider.GetOpenTelemetryTracer<TestEvent, TestResponse>();
        var nextDelegate = Substitute.For<LambdaInvocationDelegate>();
        var wrappedDelegate = middleware(nextDelegate);

        var mocks = CreateMocks();

        mocks.EventFeature.GetEvent(mocks.Context).Returns(new TestEvent());
        mocks.ResponseFeature.GetResponse().Returns(new object()); // Wrong type

        nextDelegate(Arg.Any<ILambdaHostContext>()).Returns(Task.CompletedTask);

        // Act
        var action = async () => await wrappedDelegate(mocks.Context);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetOpenTelemetryTracer_WithValidEventAndResponse_CallsNextDelegate()
    {
        // Arrange
        var middleware = _serviceProvider.GetOpenTelemetryTracer<TestEvent, TestResponse>();
        var nextDelegate = Substitute.For<LambdaInvocationDelegate>();
        var wrappedDelegate = middleware(nextDelegate);

        var testEvent = new TestEvent();
        var testResponse = new TestResponse();
        var mocks = CreateMocks();

        mocks.EventFeature.GetEvent(mocks.Context).Returns(testEvent);
        mocks.ResponseFeature.GetResponse().Returns(testResponse);

        nextDelegate(Arg.Any<ILambdaHostContext>()).Returns(Task.CompletedTask);

        // Act
        await wrappedDelegate(mocks.Context);

        // Assert
        await nextDelegate.Received(1)(Arg.Any<ILambdaHostContext>());
    }

    #endregion

    #region GetOpenTelemetryTracerNoEvent<TResponse>

    [Fact]
    public void GetOpenTelemetryTracerNoEvent_WithNullServiceProvider_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceProvider? nullProvider = null;

        // Act
        var action = () => nullProvider!.GetOpenTelemetryTracerNoEvent<TestResponse>();

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void GetOpenTelemetryTracerNoEvent_WithoutTracerProvider_ThrowsInvalidOperationException()
    {
        // Arrange
        _serviceProvider.GetService(typeof(TracerProvider)).Returns(null);

        // Act
        var action = () => _serviceProvider.GetOpenTelemetryTracerNoEvent<TestResponse>();

        // Assert
        action.Should().ThrowExactly<InvalidOperationException>();
    }

    [Fact]
    public void GetOpenTelemetryTracerNoEvent_ReturnsMiddlewareFunction()
    {
        // Act
        var middleware = _serviceProvider.GetOpenTelemetryTracerNoEvent<TestResponse>();

        // Assert
        middleware.Should().NotBeNull();
        middleware.Should().BeOfType<Func<LambdaInvocationDelegate, LambdaInvocationDelegate>>();
    }

    [Fact]
    public async Task GetOpenTelemetryTracerNoEvent_WithIncorrectResponseType_ThrowsInvalidOperationException()
    {
        // Arrange
        var middleware = _serviceProvider.GetOpenTelemetryTracerNoEvent<TestResponse>();
        var nextDelegate = Substitute.For<LambdaInvocationDelegate>();
        var wrappedDelegate = middleware(nextDelegate);

        var mocks = CreateMocks();

        mocks.EventFeature.GetEvent(mocks.Context).Returns(new object());
        mocks.ResponseFeature.GetResponse().Returns(new object()); // Wrong type

        nextDelegate(Arg.Any<ILambdaHostContext>()).Returns(Task.CompletedTask);

        // Act
        var action = async () => await wrappedDelegate(mocks.Context);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetOpenTelemetryTracerNoEvent_WithValidResponse_CallsNextDelegate()
    {
        // Arrange
        var middleware = _serviceProvider.GetOpenTelemetryTracerNoEvent<TestResponse>();
        var nextDelegate = Substitute.For<LambdaInvocationDelegate>();
        var wrappedDelegate = middleware(nextDelegate);

        var testResponse = new TestResponse();
        var mocks = CreateMocks();

        mocks.EventFeature.GetEvent(mocks.Context).Returns(new object());
        mocks.ResponseFeature.GetResponse().Returns(testResponse);

        nextDelegate(Arg.Any<ILambdaHostContext>()).Returns(Task.CompletedTask);

        // Act
        await wrappedDelegate(mocks.Context);

        // Assert
        await nextDelegate.Received(1)(Arg.Any<ILambdaHostContext>());
    }

    #endregion

    #region GetOpenTelemetryTracerNoResponse<TEvent>

    [Fact]
    public void GetOpenTelemetryTracerNoResponse_WithNullServiceProvider_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceProvider? nullProvider = null;

        // Act
        var action = () => nullProvider!.GetOpenTelemetryTracerNoResponse<TestEvent>();

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void GetOpenTelemetryTracerNoResponse_WithoutTracerProvider_ThrowsInvalidOperationException()
    {
        // Arrange
        _serviceProvider.GetService(typeof(TracerProvider)).Returns(null);

        // Act
        var action = () => _serviceProvider.GetOpenTelemetryTracerNoResponse<TestEvent>();

        // Assert
        action.Should().ThrowExactly<InvalidOperationException>();
    }

    [Fact]
    public void GetOpenTelemetryTracerNoResponse_ReturnsMiddlewareFunction()
    {
        // Act
        var middleware = _serviceProvider.GetOpenTelemetryTracerNoResponse<TestEvent>();

        // Assert
        middleware.Should().NotBeNull();
        middleware.Should().BeOfType<Func<LambdaInvocationDelegate, LambdaInvocationDelegate>>();
    }

    [Fact]
    public async Task GetOpenTelemetryTracerNoResponse_WithIncorrectEventType_ThrowsInvalidOperationException()
    {
        // Arrange
        var middleware = _serviceProvider.GetOpenTelemetryTracerNoResponse<TestEvent>();
        var nextDelegate = Substitute.For<LambdaInvocationDelegate>();
        var wrappedDelegate = middleware(nextDelegate);

        var mocks = CreateMocks();

        mocks.EventFeature.GetEvent(mocks.Context).Returns(new object()); // Wrong type
        mocks.ResponseFeature.GetResponse().Returns(new object());

        // Act
        var action = async () => await wrappedDelegate(mocks.Context);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetOpenTelemetryTracerNoResponse_WithValidEvent_CallsNextDelegate()
    {
        // Arrange
        var middleware = _serviceProvider.GetOpenTelemetryTracerNoResponse<TestEvent>();
        var nextDelegate = Substitute.For<LambdaInvocationDelegate>();
        var wrappedDelegate = middleware(nextDelegate);

        var testEvent = new TestEvent();
        var mocks = CreateMocks();

        mocks.EventFeature.GetEvent(mocks.Context).Returns(testEvent);
        mocks.ResponseFeature.GetResponse().Returns(new object());

        nextDelegate(Arg.Any<ILambdaHostContext>()).Returns(Task.CompletedTask);

        // Act
        await wrappedDelegate(mocks.Context);

        // Assert
        await nextDelegate.Received(1)(Arg.Any<ILambdaHostContext>());
    }

    #endregion

    #region GetOpenTelemetryTracerNoEventNoResponse

    [Fact]
    public void GetOpenTelemetryTracerNoEventNoResponse_WithNullServiceProvider_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceProvider? nullProvider = null;

        // Act
        var action = () => nullProvider!.GetOpenTelemetryTracerNoEventNoResponse();

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void GetOpenTelemetryTracerNoEventNoResponse_WithoutTracerProvider_ThrowsInvalidOperationException()
    {
        // Arrange
        _serviceProvider.GetService(typeof(TracerProvider)).Returns(null);

        // Act
        var action = () => _serviceProvider.GetOpenTelemetryTracerNoEventNoResponse();

        // Assert
        action.Should().ThrowExactly<InvalidOperationException>();
    }

    [Fact]
    public void GetOpenTelemetryTracerNoEventNoResponse_ReturnsMiddlewareFunction()
    {
        // Act
        var middleware = _serviceProvider.GetOpenTelemetryTracerNoEventNoResponse();

        // Assert
        middleware.Should().NotBeNull();
        middleware.Should().BeOfType<Func<LambdaInvocationDelegate, LambdaInvocationDelegate>>();
    }

    [Fact]
    public async Task GetOpenTelemetryTracerNoEventNoResponse_WithAnyEventAndResponse_CallsNextDelegate()
    {
        // Arrange
        var middleware = _serviceProvider.GetOpenTelemetryTracerNoEventNoResponse();
        var nextDelegate = Substitute.For<LambdaInvocationDelegate>();
        var wrappedDelegate = middleware(nextDelegate);

        var mocks = CreateMocks();

        mocks.EventFeature.GetEvent(mocks.Context).Returns(new object());
        mocks.ResponseFeature.GetResponse().Returns(new object());

        nextDelegate(Arg.Any<ILambdaHostContext>()).Returns(Task.CompletedTask);

        // Act
        await wrappedDelegate(mocks.Context);

        // Assert
        await nextDelegate.Received(1)(Arg.Any<ILambdaHostContext>());
    }

    #endregion

    #region Test Helpers

    private record Mocks(
        ILambdaHostContext Context,
        IFeatureCollection Features,
        IEventFeature EventFeature,
        IResponseFeature ResponseFeature
    );

    private Mocks CreateMocks()
    {
        var context = Substitute.For<ILambdaHostContext>();
        var features = Substitute.For<IFeatureCollection>();
        var eventFeature = Substitute.For<IEventFeature>();
        var responseFeature = Substitute.For<IResponseFeature>();

        context.Features.Returns(features);

        features.Get<IEventFeature>().Returns(eventFeature);
        features.Get<IResponseFeature>().Returns(responseFeature);

        return new Mocks(context, features, eventFeature, responseFeature);
    }

    private class TestEvent { }

    private class TestResponse { }

    #endregion
}
