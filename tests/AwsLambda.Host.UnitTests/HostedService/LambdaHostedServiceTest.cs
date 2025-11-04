using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using AwesomeAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace AwsLambda.Host.UnitTests.HostedService;

[TestSubject(typeof(LambdaHostedService))]
public class LambdaHostedServiceTest
{
    private readonly ILambdaBootstrapOrchestrator _bootstrap =
        Substitute.For<ILambdaBootstrapOrchestrator>();

    private readonly ILambdaHandlerFactory _handlerFactory =
        Substitute.For<ILambdaHandlerFactory>();

    private readonly ILambdaLifecycleOrchestrator _lifecycle =
        Substitute.For<ILambdaLifecycleOrchestrator>();

    private readonly IHostApplicationLifetime _lifetime =
        Substitute.For<IHostApplicationLifetime>();

    // ============================================================================
    // Constructor Validation Tests (4 tests)
    // ============================================================================

    [Fact]
    public void Constructor_WithNullBootstrap_ThrowsArgumentNullException()
    {
        // Act
        var action = () => new LambdaHostedService(null!, _handlerFactory, _lifetime, _lifecycle);

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("bootstrap");
    }

    [Fact]
    public void Constructor_WithNullHandlerFactory_ThrowsArgumentNullException()
    {
        // Act
        var action = () => new LambdaHostedService(_bootstrap, null!, _lifetime, _lifecycle);

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("handlerFactory");
    }

    [Fact]
    public void Constructor_WithNullLifetime_ThrowsArgumentNullException()
    {
        // Act
        var action = () => new LambdaHostedService(_bootstrap, _handlerFactory, null!, _lifecycle);

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("lifetime");
    }

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstanceSuccessfully()
    {
        // Act
        var service = new LambdaHostedService(_bootstrap, _handlerFactory, _lifetime, _lifecycle);

        // Assert
        service.Should().NotBeNull();
    }

    // ============================================================================
    // StartAsync Tests (5 tests)
    // ============================================================================

    [Fact]
    public void StartAsync_WithRunningTask_ReturnsCompletedTask()
    {
        // Arrange
        SetupHandlerFactory();
        SetupLifecycle();
        var bootstrapTcs = SetupBootstrapRunAsync();
        var service = new LambdaHostedService(_bootstrap, _handlerFactory, _lifetime, _lifecycle);

        // Act
        var result = service.StartAsync(CancellationToken.None);

        // Assert
        result.Should().Be(Task.CompletedTask);
        bootstrapTcs.SetResult();
    }

    [Fact]
    public async Task StartAsync_WithAlreadyCompletedExecuteAsync_ReturnsFaultedTask()
    {
        // Arrange
        var testException = new InvalidOperationException("Test error");
        _handlerFactory.CreateHandler(Arg.Any<CancellationToken>()).Throws(testException);

        var service = new LambdaHostedService(_bootstrap, _handlerFactory, _lifetime, _lifecycle);

        // Act
        var result = service.StartAsync(CancellationToken.None);

        // Assert
        var act = async () => await result;
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task StartAsync_StoresExecuteTaskForLaterUse()
    {
        // Arrange
        SetupHandlerFactory();
        var executeTcs = SetupBootstrapRunAsync();
        var stopApplicationCalled = TrackStopApplicationCall();

        var service = new LambdaHostedService(_bootstrap, _handlerFactory, _lifetime, _lifecycle);

        // Act
        var startTask = service.StartAsync(CancellationToken.None);

        // Assert
        startTask.Should().Be(Task.CompletedTask);

        executeTcs.SetResult();
        await stopApplicationCalled.Task;
        stopApplicationCalled.Task.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task StartAsync_CallsHandlerFactoryWithCancellationToken()
    {
        // Arrange
        SetupHandlerFactory();
        SetupLifecycle();
        var bootstrapTcs = SetupBootstrapRunAsync();
        var service = new LambdaHostedService(_bootstrap, _handlerFactory, _lifetime, _lifecycle);

        // Act
        await service.StartAsync(CancellationToken.None);

        // Assert
        _handlerFactory.Received(1).CreateHandler(Arg.Any<CancellationToken>());
        bootstrapTcs.SetResult();
    }

    // ============================================================================
    // StopAsync Tests (7 tests)
    // ============================================================================

    [Fact]
    public async Task StopAsync_WithoutStart_ReturnsSuccessfully()
    {
        // Arrange
        SetupLifecycle();
        var service = new LambdaHostedService(_bootstrap, _handlerFactory, _lifetime, _lifecycle);

        // Act & Assert
        var action = async () => await service.StopAsync(CancellationToken.None);
        await action.Should().NotThrowAsync<Exception>();
    }

    [Fact]
    public async Task StopAsync_WaitsForExecuteTaskCompletion()
    {
        // Arrange
        SetupHandlerFactory();
        SetupLifecycle();
        var bootstrapTcs = SetupBootstrapRunAsync();
        var service = new LambdaHostedService(_bootstrap, _handlerFactory, _lifetime, _lifecycle);

        await service.StartAsync(CancellationToken.None);

        var stopTask = service.StopAsync(CancellationToken.None);

        // Act & Assert
        stopTask.IsCompleted.Should().BeFalse();
        bootstrapTcs.SetResult();

        await stopTask;
        stopTask.IsCompletedSuccessfully.Should().BeTrue();
    }

    [Fact]
    public async Task StopAsync_WithTimeoutToken_ThrowsAggregateExceptionWithOperationCanceledException()
    {
        // Arrange
        SetupHandlerFactory();
        SetupLifecycle();
        var bootstrapTcs = SetupBootstrapRunAsync();
        var service = new LambdaHostedService(_bootstrap, _handlerFactory, _lifetime, _lifecycle);

        await service.StartAsync(CancellationToken.None);

        using var stopCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        // Act & Assert
        var act = async () => await service.StopAsync(stopCts.Token);
        var ex = await act.Should().ThrowAsync<AggregateException>();
        ex.Which.InnerExceptions.Should().Contain(ie => ie is OperationCanceledException);
    }

    [Fact]
    public async Task StopAsync_WhenBootstrapFails_ThrowsAggregateExceptionWithInnerException()
    {
        // Arrange
        SetupHandlerFactory();
        var testException = new InvalidOperationException("Bootstrap error");
        var bootstrapTcs = SetupBootstrapRunAsync();
        var service = new LambdaHostedService(_bootstrap, _handlerFactory, _lifetime, _lifecycle);

        await service.StartAsync(CancellationToken.None);

        var stopTask = service.StopAsync(CancellationToken.None);

        // Act
        bootstrapTcs.SetException(testException);

        // Assert
        var act = async () => await stopTask;
        var ex = await act.Should().ThrowAsync<AggregateException>();
        ex.Which.InnerExceptions.Should().Contain(ie => ie is InvalidOperationException);
    }

    [Fact]
    public async Task StopAsync_WhenBootstrapCompletesSuccessfully_CompletesWithoutException()
    {
        // Arrange
        SetupHandlerFactory();
        SetupLifecycle();
        var bootstrapTcs = SetupBootstrapRunAsync();
        var service = new LambdaHostedService(_bootstrap, _handlerFactory, _lifetime, _lifecycle);

        await service.StartAsync(CancellationToken.None);

        var stopTask = service.StopAsync(CancellationToken.None);

        // Act
        bootstrapTcs.SetResult();

        // Assert
        await stopTask;
        stopTask.IsCompletedSuccessfully.Should().BeTrue();
    }

    [Fact]
    public async Task StopAsync_AfterDispose_HandlesGracefully()
    {
        // Arrange
        SetupHandlerFactory();
        SetupLifecycle();
        var bootstrapTcs = SetupBootstrapRunAsync();
        var service = new LambdaHostedService(_bootstrap, _handlerFactory, _lifetime, _lifecycle);

        await service.StartAsync(CancellationToken.None);
        service.Dispose();

        bootstrapTcs.SetResult();

        // Act & Assert
        var action = async () => await service.StopAsync(CancellationToken.None);
        await action.Should().NotThrowAsync<Exception>();
    }

    [Fact]
    public async Task StopAsync_WhenOnShutdownReturnsSingleError_ThrowsAggregateExceptionWithShutdownError()
    {
        // Arrange
        SetupHandlerFactory();
        var shutdownException = new InvalidOperationException("Shutdown handler error");
        _lifecycle
            .OnShutdown(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<Exception>>([shutdownException]));

        var bootstrapTcs = SetupBootstrapRunAsync();
        var service = new LambdaHostedService(_bootstrap, _handlerFactory, _lifetime, _lifecycle);

        await service.StartAsync(CancellationToken.None);

        var stopTask = service.StopAsync(CancellationToken.None);

        // Act
        bootstrapTcs.SetResult();

        // Assert
        var act = async () => await stopTask;
        var ex = await act.Should().ThrowAsync<AggregateException>();
        ex.Which.InnerExceptions.Should()
            .Contain(ie =>
                ie is InvalidOperationException && ie.Message == "Shutdown handler error"
            );
    }

    [Fact]
    public async Task StopAsync_WhenOnShutdownReturnsMultipleErrors_ThrowsAggregateExceptionWithAllShutdownErrors()
    {
        // Arrange
        SetupHandlerFactory();
        var shutdownException1 = new InvalidOperationException("First shutdown error");
        var shutdownException2 = new TimeoutException("Second shutdown error");

        _lifecycle
            .OnShutdown(Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<IEnumerable<Exception>>([shutdownException1, shutdownException2])
            );

        var bootstrapTcs = SetupBootstrapRunAsync();
        var service = new LambdaHostedService(_bootstrap, _handlerFactory, _lifetime, _lifecycle);

        await service.StartAsync(CancellationToken.None);

        var stopTask = service.StopAsync(CancellationToken.None);

        // Act
        bootstrapTcs.SetResult();

        // Assert
        var act = async () => await stopTask;
        var ex = await act.Should().ThrowAsync<AggregateException>();
        ex.Which.InnerExceptions.Should().HaveCount(2);
        ex.Which.InnerExceptions.Should()
            .Contain(ie => ie is InvalidOperationException && ie.Message == "First shutdown error");
        ex.Which.InnerExceptions.Should()
            .Contain(ie => ie is TimeoutException && ie.Message == "Second shutdown error");
    }

    // ============================================================================
    // Dispose Tests (2 tests)
    // ============================================================================

    [Fact]
    public async Task Dispose_TriggersCancellationTokenSource()
    {
        // Arrange
        SetupHandlerFactory();
        SetupLifecycle();
        var bootstrapTcs = SetupBootstrapRunAsync();
        var service = new LambdaHostedService(_bootstrap, _handlerFactory, _lifetime, _lifecycle);

        await service.StartAsync(CancellationToken.None);

        // Act
        service.Dispose();

        // Assert
        bootstrapTcs.SetResult();
        var action = async () => await service.StopAsync(CancellationToken.None);
        await action.Should().NotThrowAsync<Exception>();
    }

    [Fact]
    public void Dispose_WithoutStartAsync_DoesNotThrow()
    {
        // Arrange
        SetupLifecycle();
        var service = new LambdaHostedService(_bootstrap, _handlerFactory, _lifetime, _lifecycle);

        // Act & Assert
        var action = () => service.Dispose();
        action.Should().NotThrow();
    }

    // ============================================================================
    // Integration Tests (4 tests)
    // ============================================================================

    [Fact]
    public async Task StartAsync_And_StopAsync_FullLifecycle()
    {
        // Arrange
        SetupHandlerFactory();
        SetupLifecycle();
        var bootstrapTcs = SetupBootstrapRunAsync();
        var service = new LambdaHostedService(_bootstrap, _handlerFactory, _lifetime, _lifecycle);

        // Act
        await service.StartAsync(CancellationToken.None);

        var stopTask = service.StopAsync(CancellationToken.None);

        bootstrapTcs.SetResult();

        await stopTask;

        // Assert
        _lifetime.Received(1).StopApplication();
    }

    [Fact]
    public async Task StartAsync_CallsBootstrapRunAsyncWithCreatedHandler()
    {
        // Arrange
        SetupHandlerFactory();
        SetupLifecycle();
        var bootstrapTcs = SetupBootstrapRunAsync();
        var service = new LambdaHostedService(_bootstrap, _handlerFactory, _lifetime, _lifecycle);

        // Act
        await service.StartAsync(CancellationToken.None);

        // Assert
        await _bootstrap
            .Received(1)
            .RunAsync(
                CreateMockHandler(),
                Arg.Any<LambdaBootstrapInitializer?>(),
                Arg.Any<CancellationToken>()
            );
        bootstrapTcs.SetResult();
    }

    [Fact]
    public async Task ExecuteAsync_CallsStopApplicationInFinallyBlock_EvenWhenBootstrapFails()
    {
        // Arrange
        SetupHandlerFactory();
        var exceptionToThrow = new InvalidOperationException("Test error");
        var bootstrapTcs = SetupBootstrapRunAsync();
        var service = new LambdaHostedService(_bootstrap, _handlerFactory, _lifetime, _lifecycle);

        // Act
        await service.StartAsync(CancellationToken.None);
        bootstrapTcs.SetException(exceptionToThrow);

        // Assert
        var act = async () => await service.StopAsync(CancellationToken.None);
        await act.Should().ThrowAsync<AggregateException>();
        _lifetime.Received(1).StopApplication();
    }

    [Fact]
    public async Task StopAsync_CalledMultipleTimes_HandlesCorrectly()
    {
        // Arrange
        SetupHandlerFactory();
        SetupLifecycle();
        var bootstrapTcs = SetupBootstrapRunAsync();
        var service = new LambdaHostedService(_bootstrap, _handlerFactory, _lifetime, _lifecycle);

        await service.StartAsync(CancellationToken.None);

        // Act - first StopAsync
        var stop1 = service.StopAsync(CancellationToken.None);

        bootstrapTcs.SetResult();
        await stop1;

        // Second call to StopAsync after task has completed
        var stop2 = service.StopAsync(CancellationToken.None);

        // Assert
        var action = async () => await stop2;
        await action.Should().NotThrowAsync<Exception>();
    }

    // ============================================================================
    // Helper Methods
    // ============================================================================

    /// <summary>Sets up the handler factory to return a mock Lambda handler.</summary>
    private void SetupHandlerFactory()
    {
        var handler = CreateMockHandler();
        _handlerFactory.CreateHandler(Arg.Any<CancellationToken>()).Returns(handler);
    }

    /// <summary>Sets up the lifecycle orchestrator to return an empty exception list on shutdown.</summary>
    private void SetupLifecycle() =>
        _lifecycle
            .OnShutdown(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<Exception>>([]));

    /// <summary>
    ///     Sets up the bootstrap orchestrator to run asynchronously. Returns a TaskCompletionSource
    ///     to control when the bootstrap completes.
    /// </summary>
    private TaskCompletionSource SetupBootstrapRunAsync()
    {
        var tcs = new TaskCompletionSource();
        _bootstrap
            .RunAsync(
                Arg.Any<Func<Stream, ILambdaContext, Task<Stream>>>(),
                Arg.Any<LambdaBootstrapInitializer?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(tcs.Task);
        return tcs;
    }

    /// <summary>
    ///     Tracks when the lifetime's StopApplication method is called. Returns a
    ///     TaskCompletionSource that completes when StopApplication is invoked.
    /// </summary>
    private TaskCompletionSource TrackStopApplicationCall()
    {
        var stopApplicationCalled = new TaskCompletionSource();
        _lifetime
            .WhenForAnyArgs(x => x.StopApplication())
            .Do(_ => stopApplicationCalled.TrySetResult());
        return stopApplicationCalled;
    }

    /// <summary>Creates a mock Lambda handler delegate.</summary>
    private static Func<Stream, ILambdaContext, Task<Stream>> CreateMockHandler() =>
        async (stream, context) =>
        {
            await Task.CompletedTask;
            return new MemoryStream();
        };
}
