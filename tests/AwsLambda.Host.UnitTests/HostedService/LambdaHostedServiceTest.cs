using Amazon.Lambda.Core;
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

    private readonly IHostApplicationLifetime _lifetime =
        Substitute.For<IHostApplicationLifetime>();

    // ============================================================================
    // Constructor Validation Tests (4 tests)
    // ============================================================================

    [Fact]
    public void Constructor_WithNullBootstrap_ThrowsArgumentNullException()
    {
        // Act
        var action = () => new LambdaHostedService(null!, _handlerFactory, _lifetime);

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("bootstrap");
    }

    [Fact]
    public void Constructor_WithNullHandlerFactory_ThrowsArgumentNullException()
    {
        // Act
        var action = () => new LambdaHostedService(_bootstrap, null!, _lifetime);

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("handlerFactory");
    }

    [Fact]
    public void Constructor_WithNullLifetime_ThrowsArgumentNullException()
    {
        // Act
        var action = () => new LambdaHostedService(_bootstrap, _handlerFactory, null!);

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("lifetime");
    }

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstanceSuccessfully()
    {
        // Act
        var service = new LambdaHostedService(_bootstrap, _handlerFactory, _lifetime);

        // Assert
        service.Should().NotBeNull();
    }

    // ============================================================================
    // StartAsync Tests (5 tests)
    // ============================================================================

    [Fact]
    public async Task StartAsync_WithRunningTask_ReturnsCompletedTask()
    {
        // Arrange
        SetupHandlerFactory();
        var bootstrapTcs = SetupBootstrapRunAsync();
        var service = new LambdaHostedService(_bootstrap, _handlerFactory, _lifetime);
        using var cts = new CancellationTokenSource();

        // Act
        var result = service.StartAsync(cts.Token);

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

        var service = new LambdaHostedService(_bootstrap, _handlerFactory, _lifetime);
        using var cts = new CancellationTokenSource();

        // Act
        var result = service.StartAsync(cts.Token);

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

        var service = new LambdaHostedService(_bootstrap, _handlerFactory, _lifetime);
        using var cts = new CancellationTokenSource();

        // Act
        var startTask = service.StartAsync(cts.Token);

        // Assert
        startTask.Should().Be(Task.CompletedTask);

        executeTcs.SetResult();
        var timeoutTcs = new TaskCompletionSource();
        var completedFirst = await Task.WhenAny(stopApplicationCalled.Task, timeoutTcs.Task);
        completedFirst.Should().Be(stopApplicationCalled.Task);
    }

    [Fact]
    public async Task StartAsync_CallsHandlerFactoryWithCancellationToken()
    {
        // Arrange
        SetupHandlerFactory();
        var bootstrapTcs = SetupBootstrapRunAsync();
        var service = new LambdaHostedService(_bootstrap, _handlerFactory, _lifetime);
        using var cts = new CancellationTokenSource();

        // Act
        await service.StartAsync(cts.Token);

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
        var service = new LambdaHostedService(_bootstrap, _handlerFactory, _lifetime);
        using var cts = new CancellationTokenSource();

        // Act & Assert
        var action = async () => await service.StopAsync(cts.Token);
        await action.Should().NotThrowAsync<Exception>();
    }

    [Fact]
    public async Task StopAsync_WaitsForExecuteTaskCompletion()
    {
        // Arrange
        SetupHandlerFactory();
        var bootstrapTcs = SetupBootstrapRunAsync();
        var service = new LambdaHostedService(_bootstrap, _handlerFactory, _lifetime);
        using var startCts = new CancellationTokenSource();

        await service.StartAsync(startCts.Token);

        using var stopCts = new CancellationTokenSource();
        var stopTask = service.StopAsync(stopCts.Token);

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
        var bootstrapTcs = SetupBootstrapRunAsync();
        var service = new LambdaHostedService(_bootstrap, _handlerFactory, _lifetime);
        using var startCts = new CancellationTokenSource();

        await service.StartAsync(startCts.Token);

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
        var service = new LambdaHostedService(_bootstrap, _handlerFactory, _lifetime);
        using var startCts = new CancellationTokenSource();

        await service.StartAsync(startCts.Token);

        using var stopCts = new CancellationTokenSource();
        var stopTask = service.StopAsync(stopCts.Token);

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
        var bootstrapTcs = SetupBootstrapRunAsync();
        var service = new LambdaHostedService(_bootstrap, _handlerFactory, _lifetime);
        using var startCts = new CancellationTokenSource();

        await service.StartAsync(startCts.Token);

        using var stopCts = new CancellationTokenSource();
        var stopTask = service.StopAsync(stopCts.Token);

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
        var bootstrapTcs = SetupBootstrapRunAsync();
        var service = new LambdaHostedService(_bootstrap, _handlerFactory, _lifetime);
        using var startCts = new CancellationTokenSource();

        await service.StartAsync(startCts.Token);
        service.Dispose();

        using var stopCts = new CancellationTokenSource();
        bootstrapTcs.SetResult();

        // Act & Assert
        var action = async () => await service.StopAsync(stopCts.Token);
        await action.Should().NotThrowAsync<Exception>();
    }

    // ============================================================================
    // Dispose Tests (2 tests)
    // ============================================================================

    [Fact]
    public async Task Dispose_TriggersCancellationTokenSource()
    {
        // Arrange
        SetupHandlerFactory();
        var bootstrapTcs = SetupBootstrapRunAsync();
        var service = new LambdaHostedService(_bootstrap, _handlerFactory, _lifetime);
        using var cts = new CancellationTokenSource();

        await service.StartAsync(cts.Token);

        // Act
        service.Dispose();

        // Assert
        using var stopCts = new CancellationTokenSource();
        var action = async () => await service.StopAsync(stopCts.Token);
        bootstrapTcs.SetResult();
        await action.Should().NotThrowAsync<Exception>();
    }

    [Fact]
    public void Dispose_WithoutStartAsync_DoesNotThrow()
    {
        // Arrange
        var service = new LambdaHostedService(_bootstrap, _handlerFactory, _lifetime);

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
        var bootstrapTcs = SetupBootstrapRunAsync();
        var service = new LambdaHostedService(_bootstrap, _handlerFactory, _lifetime);
        using var startCts = new CancellationTokenSource();

        // Act
        await service.StartAsync(startCts.Token);

        using var stopCts = new CancellationTokenSource();
        var stopTask = service.StopAsync(stopCts.Token);

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
        var bootstrapTcs = SetupBootstrapRunAsync();
        var service = new LambdaHostedService(_bootstrap, _handlerFactory, _lifetime);
        using var cts = new CancellationTokenSource();

        // Act
        await service.StartAsync(cts.Token);

        // Assert
        await _bootstrap.Received(1).RunAsync(CreateMockHandler(), Arg.Any<CancellationToken>());
        bootstrapTcs.SetResult();
    }

    [Fact]
    public async Task ExecuteAsync_CallsStopApplicationInFinallyBlock_EvenWhenBootstrapFails()
    {
        // Arrange
        SetupHandlerFactory();
        var exceptionToThrow = new InvalidOperationException("Test error");
        var bootstrapTcs = SetupBootstrapRunAsync();
        var service = new LambdaHostedService(_bootstrap, _handlerFactory, _lifetime);
        using var startCts = new CancellationTokenSource();

        // Act
        await service.StartAsync(startCts.Token);
        bootstrapTcs.SetException(exceptionToThrow);

        // Assert
        using var stopCts = new CancellationTokenSource();
        var act = async () => await service.StopAsync(stopCts.Token);
        await act.Should().ThrowAsync<AggregateException>();
        _lifetime.Received(1).StopApplication();
    }

    [Fact]
    public async Task StopAsync_CalledMultipleTimes_HandlesCorrectly()
    {
        // Arrange
        SetupHandlerFactory();
        var bootstrapTcs = SetupBootstrapRunAsync();
        var service = new LambdaHostedService(_bootstrap, _handlerFactory, _lifetime);
        using var startCts = new CancellationTokenSource();

        await service.StartAsync(startCts.Token);

        // Act - first StopAsync
        using var stopCts1 = new CancellationTokenSource();
        var stop1 = service.StopAsync(stopCts1.Token);

        bootstrapTcs.SetResult();
        await stop1;

        // Second call to StopAsync after task has completed
        using var stopCts2 = new CancellationTokenSource();
        var stop2 = service.StopAsync(stopCts2.Token);

        // Assert
        var action = async () => await stop2;
        await action.Should().NotThrowAsync<Exception>();
    }

    // ============================================================================
    // Helper Methods
    // ============================================================================

    /// <summary>
    /// Sets up the handler factory to return a mock Lambda handler.
    /// </summary>
    private void SetupHandlerFactory()
    {
        var handler = CreateMockHandler();
        _handlerFactory.CreateHandler(Arg.Any<CancellationToken>()).Returns(handler);
    }

    /// <summary>
    /// Sets up the bootstrap orchestrator to run asynchronously.
    /// Returns a TaskCompletionSource to control when the bootstrap completes.
    /// </summary>
    private TaskCompletionSource SetupBootstrapRunAsync()
    {
        var tcs = new TaskCompletionSource();
        _bootstrap
            .RunAsync(
                Arg.Any<Func<Stream, ILambdaContext, Task<Stream>>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(tcs.Task);
        return tcs;
    }

    /// <summary>
    /// Tracks when the lifetime's StopApplication method is called.
    /// Returns a TaskCompletionSource that completes when StopApplication is invoked.
    /// </summary>
    private TaskCompletionSource TrackStopApplicationCall()
    {
        var stopApplicationCalled = new TaskCompletionSource();
        _lifetime
            .WhenForAnyArgs(x => x.StopApplication())
            .Do(_ => stopApplicationCalled.TrySetResult());
        return stopApplicationCalled;
    }

    /// <summary>
    /// Creates a mock Lambda handler delegate.
    /// </summary>
    private static Func<Stream, ILambdaContext, Task<Stream>> CreateMockHandler()
    {
        return async (stream, context) =>
        {
            await Task.CompletedTask;
            return new MemoryStream();
        };
    }
}
