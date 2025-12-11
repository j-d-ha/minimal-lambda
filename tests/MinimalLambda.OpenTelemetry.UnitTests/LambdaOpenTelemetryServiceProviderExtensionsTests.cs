using Microsoft.Extensions.DependencyInjection;
using MinimalLambda.UnitTests;
using OpenTelemetry.Trace;

namespace MinimalLambda.OpenTelemetry.UnitTests;

[TestSubject(typeof(LambdaOpenTelemetryServiceProviderExtensions))]
public class LambdaOpenTelemetryServiceProviderExtensionsTests
{
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

    [Theory]
    [AutoNSubstituteData]
    public void GetOpenTelemetryTracer_WithoutTracerProvider_ThrowsInvalidOperationException(
        [Frozen] IServiceProvider serviceProvider
    )
    {
        // Arrange
        serviceProvider.GetService(typeof(TracerProvider)).Returns(null);

        // Act
        var action = () => serviceProvider.GetOpenTelemetryTracer<TestEvent, TestResponse>();

        // Assert
        action.Should().ThrowExactly<InvalidOperationException>();
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetOpenTelemetryTracer_ReturnsMiddlewareFunction(
        [Frozen] IServiceProvider serviceProvider,
        TracerProvider tracerProvider
    )
    {
        // Arrange
        serviceProvider.GetService(typeof(TracerProvider)).Returns(tracerProvider);

        // Act
        var middleware = serviceProvider.GetOpenTelemetryTracer<TestEvent, TestResponse>();

        // Assert
        middleware.Should().NotBeNull();
        middleware.Should().BeOfType<Func<LambdaInvocationDelegate, LambdaInvocationDelegate>>();
    }

    [Theory]
    [AutoNSubstituteData]
    public async Task GetOpenTelemetryTracer_WithIncorrectEventType_ThrowsInvalidOperationException(
        [Frozen] IServiceProvider serviceProvider,
        TracerProvider tracerProvider,
        ILambdaHostContext context,
        IFeatureCollection features,
        IEventFeature eventFeature,
        IResponseFeature responseFeature
    )
    {
        // Arrange
        serviceProvider.GetService(typeof(TracerProvider)).Returns(tracerProvider);
        var middleware = serviceProvider.GetOpenTelemetryTracer<TestEvent, TestResponse>();
        var nextDelegate = Substitute.For<LambdaInvocationDelegate>();
        var wrappedDelegate = middleware(nextDelegate);

        context.Features.Returns(features);
        features.Get<IEventFeature>().Returns(eventFeature);
        features.Get<IResponseFeature>().Returns(responseFeature);
        eventFeature.GetEvent(context).Returns(new object());

        // Act
        var action = async () => await wrappedDelegate(context);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    [Theory]
    [AutoNSubstituteData]
    public async Task GetOpenTelemetryTracer_WithIncorrectResponseType_ThrowsInvalidOperationException(
        [Frozen] IServiceProvider serviceProvider,
        TracerProvider tracerProvider,
        ILambdaHostContext context,
        IFeatureCollection features,
        IEventFeature eventFeature,
        IResponseFeature responseFeature
    )
    {
        // Arrange
        serviceProvider.GetService(typeof(TracerProvider)).Returns(tracerProvider);
        var middleware = serviceProvider.GetOpenTelemetryTracer<TestEvent, TestResponse>();
        var nextDelegate = Substitute.For<LambdaInvocationDelegate>();
        var wrappedDelegate = middleware(nextDelegate);

        context.Features.Returns(features);
        features.Get<IEventFeature>().Returns(eventFeature);
        features.Get<IResponseFeature>().Returns(responseFeature);
        eventFeature.GetEvent(context).Returns(new TestEvent());
        responseFeature.GetResponse().Returns(new object()); // Wrong type

        nextDelegate(Arg.Any<ILambdaHostContext>()).Returns(Task.CompletedTask);

        // Act
        var action = async () => await wrappedDelegate(context);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    [Theory]
    [AutoNSubstituteData]
    public async Task GetOpenTelemetryTracer_WithValidEventAndResponse_CallsNextDelegate(
        [Frozen] IServiceProvider serviceProvider,
        TracerProvider tracerProvider,
        ILambdaHostContext context,
        IFeatureCollection features,
        IEventFeature eventFeature,
        IResponseFeature responseFeature
    )
    {
        // Arrange
        serviceProvider.GetService(typeof(TracerProvider)).Returns(tracerProvider);
        var middleware = serviceProvider.GetOpenTelemetryTracer<TestEvent, TestResponse>();
        var nextDelegate = Substitute.For<LambdaInvocationDelegate>();
        var wrappedDelegate = middleware(nextDelegate);

        var testEvent = new TestEvent();
        var testResponse = new TestResponse();

        context.Features.Returns(features);
        features.Get<IEventFeature>().Returns(eventFeature);
        features.Get<IResponseFeature>().Returns(responseFeature);
        eventFeature.GetEvent(context).Returns(testEvent);
        responseFeature.GetResponse().Returns(testResponse);

        nextDelegate(Arg.Any<ILambdaHostContext>()).Returns(Task.CompletedTask);

        // Act
        await wrappedDelegate(context);

        // Assert
        await nextDelegate.Received(1)(Arg.Any<ILambdaHostContext>());
    }

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

    [Theory]
    [AutoNSubstituteData]
    public void GetOpenTelemetryTracerNoEvent_WithoutTracerProvider_ThrowsInvalidOperationException(
        [Frozen] IServiceProvider serviceProvider
    )
    {
        // Arrange
        serviceProvider.GetService(typeof(TracerProvider)).Returns(null);

        // Act
        var action = () => serviceProvider.GetOpenTelemetryTracerNoEvent<TestResponse>();

        // Assert
        action.Should().ThrowExactly<InvalidOperationException>();
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetOpenTelemetryTracerNoEvent_ReturnsMiddlewareFunction(
        [Frozen] IServiceProvider serviceProvider,
        TracerProvider tracerProvider
    )
    {
        // Arrange
        serviceProvider.GetService(typeof(TracerProvider)).Returns(tracerProvider);

        // Act
        var middleware = serviceProvider.GetOpenTelemetryTracerNoEvent<TestResponse>();

        // Assert
        middleware.Should().NotBeNull();
        middleware.Should().BeOfType<Func<LambdaInvocationDelegate, LambdaInvocationDelegate>>();
    }

    [Theory]
    [AutoNSubstituteData]
    public async Task GetOpenTelemetryTracerNoEvent_WithIncorrectResponseType_ThrowsInvalidOperationException(
        [Frozen] IServiceProvider serviceProvider,
        TracerProvider tracerProvider,
        ILambdaHostContext context,
        IFeatureCollection features,
        IEventFeature eventFeature,
        IResponseFeature responseFeature
    )
    {
        // Arrange
        serviceProvider.GetService(typeof(TracerProvider)).Returns(tracerProvider);
        var middleware = serviceProvider.GetOpenTelemetryTracerNoEvent<TestResponse>();
        var nextDelegate = Substitute.For<LambdaInvocationDelegate>();
        var wrappedDelegate = middleware(nextDelegate);

        context.Features.Returns(features);
        features.Get<IEventFeature>().Returns(eventFeature);
        features.Get<IResponseFeature>().Returns(responseFeature);
        eventFeature.GetEvent(context).Returns(new object());
        responseFeature.GetResponse().Returns(new object()); // Wrong type

        nextDelegate(Arg.Any<ILambdaHostContext>()).Returns(Task.CompletedTask);

        // Act
        var action = async () => await wrappedDelegate(context);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    [Theory]
    [AutoNSubstituteData]
    public async Task GetOpenTelemetryTracerNoEvent_WithValidResponse_CallsNextDelegate(
        [Frozen] IServiceProvider serviceProvider,
        TracerProvider tracerProvider,
        ILambdaHostContext context,
        IFeatureCollection features,
        IEventFeature eventFeature,
        IResponseFeature responseFeature
    )
    {
        // Arrange
        serviceProvider.GetService(typeof(TracerProvider)).Returns(tracerProvider);
        var middleware = serviceProvider.GetOpenTelemetryTracerNoEvent<TestResponse>();
        var nextDelegate = Substitute.For<LambdaInvocationDelegate>();
        var wrappedDelegate = middleware(nextDelegate);

        var testResponse = new TestResponse();

        context.Features.Returns(features);
        features.Get<IEventFeature>().Returns(eventFeature);
        features.Get<IResponseFeature>().Returns(responseFeature);
        eventFeature.GetEvent(context).Returns(new object());
        responseFeature.GetResponse().Returns(testResponse);

        nextDelegate(Arg.Any<ILambdaHostContext>()).Returns(Task.CompletedTask);

        // Act
        await wrappedDelegate(context);

        // Assert
        await nextDelegate.Received(1)(Arg.Any<ILambdaHostContext>());
    }

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

    [Theory]
    [AutoNSubstituteData]
    public void GetOpenTelemetryTracerNoResponse_WithoutTracerProvider_ThrowsInvalidOperationException(
        [Frozen] IServiceProvider serviceProvider
    )
    {
        // Arrange
        serviceProvider.GetService(typeof(TracerProvider)).Returns(null);

        // Act
        var action = () => serviceProvider.GetOpenTelemetryTracerNoResponse<TestEvent>();

        // Assert
        action.Should().ThrowExactly<InvalidOperationException>();
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetOpenTelemetryTracerNoResponse_ReturnsMiddlewareFunction(
        [Frozen] IServiceProvider serviceProvider,
        TracerProvider tracerProvider
    )
    {
        // Arrange
        serviceProvider.GetService(typeof(TracerProvider)).Returns(tracerProvider);

        // Act
        var middleware = serviceProvider.GetOpenTelemetryTracerNoResponse<TestEvent>();

        // Assert
        middleware.Should().NotBeNull();
        middleware.Should().BeOfType<Func<LambdaInvocationDelegate, LambdaInvocationDelegate>>();
    }

    [Theory]
    [AutoNSubstituteData]
    public async Task GetOpenTelemetryTracerNoResponse_WithIncorrectEventType_ThrowsInvalidOperationException(
        [Frozen] IServiceProvider serviceProvider,
        TracerProvider tracerProvider,
        ILambdaHostContext context,
        IFeatureCollection features,
        IEventFeature eventFeature,
        IResponseFeature responseFeature
    )
    {
        // Arrange
        serviceProvider.GetService(typeof(TracerProvider)).Returns(tracerProvider);
        var middleware = serviceProvider.GetOpenTelemetryTracerNoResponse<TestEvent>();
        var nextDelegate = Substitute.For<LambdaInvocationDelegate>();
        var wrappedDelegate = middleware(nextDelegate);

        context.Features.Returns(features);
        features.Get<IEventFeature>().Returns(eventFeature);
        features.Get<IResponseFeature>().Returns(responseFeature);
        eventFeature.GetEvent(context).Returns(new object()); // Wrong type
        responseFeature.GetResponse().Returns(new object());

        // Act
        var action = async () => await wrappedDelegate(context);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    [Theory]
    [AutoNSubstituteData]
    public async Task GetOpenTelemetryTracerNoResponse_WithValidEvent_CallsNextDelegate(
        [Frozen] IServiceProvider serviceProvider,
        TracerProvider tracerProvider,
        ILambdaHostContext context,
        IFeatureCollection features,
        IEventFeature eventFeature,
        IResponseFeature responseFeature
    )
    {
        // Arrange
        serviceProvider.GetService(typeof(TracerProvider)).Returns(tracerProvider);
        var middleware = serviceProvider.GetOpenTelemetryTracerNoResponse<TestEvent>();
        var nextDelegate = Substitute.For<LambdaInvocationDelegate>();
        var wrappedDelegate = middleware(nextDelegate);

        var testEvent = new TestEvent();

        context.Features.Returns(features);
        features.Get<IEventFeature>().Returns(eventFeature);
        features.Get<IResponseFeature>().Returns(responseFeature);
        eventFeature.GetEvent(context).Returns(testEvent);
        responseFeature.GetResponse().Returns(new object());

        nextDelegate(Arg.Any<ILambdaHostContext>()).Returns(Task.CompletedTask);

        // Act
        await wrappedDelegate(context);

        // Assert
        await nextDelegate.Received(1)(Arg.Any<ILambdaHostContext>());
    }

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

    [Theory]
    [AutoNSubstituteData]
    public void GetOpenTelemetryTracerNoEventNoResponse_WithoutTracerProvider_ThrowsInvalidOperationException(
        [Frozen] IServiceProvider serviceProvider
    )
    {
        // Arrange
        serviceProvider.GetService(typeof(TracerProvider)).Returns(null);

        // Act
        var action = () => serviceProvider.GetOpenTelemetryTracerNoEventNoResponse();

        // Assert
        action.Should().ThrowExactly<InvalidOperationException>();
    }

    [Theory]
    [AutoNSubstituteData]
    public void GetOpenTelemetryTracerNoEventNoResponse_ReturnsMiddlewareFunction(
        [Frozen] IServiceProvider serviceProvider,
        TracerProvider tracerProvider
    )
    {
        // Arrange
        serviceProvider.GetService(typeof(TracerProvider)).Returns(tracerProvider);

        // Act
        var middleware = serviceProvider.GetOpenTelemetryTracerNoEventNoResponse();

        // Assert
        middleware.Should().NotBeNull();
        middleware.Should().BeOfType<Func<LambdaInvocationDelegate, LambdaInvocationDelegate>>();
    }

    [Theory]
    [AutoNSubstituteData]
    public async Task GetOpenTelemetryTracerNoEventNoResponse_WithAnyEventAndResponse_CallsNextDelegate(
        [Frozen] IServiceProvider serviceProvider,
        TracerProvider tracerProvider,
        ILambdaHostContext context,
        IFeatureCollection features,
        IEventFeature eventFeature,
        IResponseFeature responseFeature
    )
    {
        // Arrange
        serviceProvider.GetService(typeof(TracerProvider)).Returns(tracerProvider);
        var middleware = serviceProvider.GetOpenTelemetryTracerNoEventNoResponse();
        var nextDelegate = Substitute.For<LambdaInvocationDelegate>();
        var wrappedDelegate = middleware(nextDelegate);

        context.Features.Returns(features);
        features.Get<IEventFeature>().Returns(eventFeature);
        features.Get<IResponseFeature>().Returns(responseFeature);
        eventFeature.GetEvent(context).Returns(new object());
        responseFeature.GetResponse().Returns(new object());

        nextDelegate(Arg.Any<ILambdaHostContext>()).Returns(Task.CompletedTask);

        // Act
        await wrappedDelegate(context);

        // Assert
        await nextDelegate.Received(1)(Arg.Any<ILambdaHostContext>());
    }

    private class TestEvent { }

    private class TestResponse { }
}
