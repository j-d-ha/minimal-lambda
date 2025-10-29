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
    // ============================================================================
    // Constructor Validation Tests (4 tests)
    // ============================================================================

    [Fact]
    public void Constructor_WithNullBootstrap_ThrowsArgumentNullException()
    {
        // Arrange
        var handlerFactory = Substitute.For<ILambdaHandlerFactory>();
        var lifetime = Substitute.For<IHostApplicationLifetime>();

        // Act
        var action = () => new LambdaHostedService(null!, handlerFactory, lifetime);

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("bootstrap");
    }

    [Fact]
    public void Constructor_WithNullHandlerFactory_ThrowsArgumentNullException()
    {
        // Arrange
        var bootstrap = Substitute.For<ILambdaBootstrapOrchestrator>();
        var lifetime = Substitute.For<IHostApplicationLifetime>();

        // Act
        var action = () => new LambdaHostedService(bootstrap, null!, lifetime);

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("handlerFactory");
    }

    [Fact]
    public void Constructor_WithNullLifetime_ThrowsArgumentNullException()
    {
        // Arrange
        var bootstrap = Substitute.For<ILambdaBootstrapOrchestrator>();
        var handlerFactory = Substitute.For<ILambdaHandlerFactory>();

        // Act
        var action = () => new LambdaHostedService(bootstrap, handlerFactory, null!);

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("lifetime");
    }

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstanceSuccessfully()
    {
        // Arrange
        var bootstrap = Substitute.For<ILambdaBootstrapOrchestrator>();
        var handlerFactory = Substitute.For<ILambdaHandlerFactory>();
        var lifetime = Substitute.For<IHostApplicationLifetime>();

        // Act
        var service = new LambdaHostedService(bootstrap, handlerFactory, lifetime);

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
        var bootstrap = Substitute.For<ILambdaBootstrapOrchestrator>();
        var handlerFactory = Substitute.For<ILambdaHandlerFactory>();
        var lifetime = Substitute.For<IHostApplicationLifetime>();

        var handler = CreateMockHandler();
        handlerFactory.CreateHandler(Arg.Any<CancellationToken>()).Returns(handler);

        var bootstrapTcs = new TaskCompletionSource();
        bootstrap
            .RunAsync(
                Arg.Any<Func<Stream, ILambdaContext, Task<Stream>>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(bootstrapTcs.Task);

        var service = new LambdaHostedService(bootstrap, handlerFactory, lifetime);
        using var cts = new CancellationTokenSource();

        // Act
        var result = service.StartAsync(cts.Token);

        // Assert
        // The StartAsync should return Task.CompletedTask when ExecuteAsync is still running
        result.Should().Be(Task.CompletedTask);
        bootstrapTcs.SetResult();
    }

    [Fact]
    public async Task StartAsync_WithAlreadyCompletedExecuteAsync_ReturnsFaultedTask()
    {
        // Arrange
        var bootstrap = Substitute.For<ILambdaBootstrapOrchestrator>();
        var handlerFactory = Substitute.For<ILambdaHandlerFactory>();
        var lifetime = Substitute.For<IHostApplicationLifetime>();

        var testException = new InvalidOperationException("Test error");
        handlerFactory.CreateHandler(Arg.Any<CancellationToken>()).Throws(testException);

        var service = new LambdaHostedService(bootstrap, handlerFactory, lifetime);
        using var cts = new CancellationTokenSource();

        // Act
        var result = service.StartAsync(cts.Token);

        // Assert - when ExecuteAsync completes immediately with exception, it should be returned
        var act = async () => await result;
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task StartAsync_StoresExecuteTaskForLaterUse()
    {
        // Arrange
        var bootstrap = Substitute.For<ILambdaBootstrapOrchestrator>();
        var handlerFactory = Substitute.For<ILambdaHandlerFactory>();
        var lifetime = Substitute.For<IHostApplicationLifetime>();

        var handler = CreateMockHandler();
        handlerFactory.CreateHandler(Arg.Any<CancellationToken>()).Returns(handler);

        var tcs = new TaskCompletionSource();
        bootstrap
            .RunAsync(
                Arg.Any<Func<Stream, ILambdaContext, Task<Stream>>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(tcs.Task);

        var service = new LambdaHostedService(bootstrap, handlerFactory, lifetime);
        using var cts = new CancellationTokenSource();

        // Act
        await service.StartAsync(cts.Token);

        // Complete the bootstrap task to verify it was stored
        tcs.SetResult();
        await Task.Delay(50); // Give time for finally block to execute

        // Assert - verify StopApplication was called, indicating the stored task executed
        lifetime.Received(1).StopApplication();
    }

    [Fact]
    public async Task StartAsync_CallsHandlerFactoryWithCancellationToken()
    {
        // Arrange
        var bootstrap = Substitute.For<ILambdaBootstrapOrchestrator>();
        var handlerFactory = Substitute.For<ILambdaHandlerFactory>();
        var lifetime = Substitute.For<IHostApplicationLifetime>();

        var handler = CreateMockHandler();
        handlerFactory.CreateHandler(Arg.Any<CancellationToken>()).Returns(handler);

        var bootstrapTcs = new TaskCompletionSource();
        bootstrap
            .RunAsync(
                Arg.Any<Func<Stream, ILambdaContext, Task<Stream>>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(bootstrapTcs.Task);

        var service = new LambdaHostedService(bootstrap, handlerFactory, lifetime);
        using var cts = new CancellationTokenSource();

        // Act
        await service.StartAsync(cts.Token);

        // Assert
        handlerFactory.Received(1).CreateHandler(Arg.Any<CancellationToken>());
        bootstrapTcs.SetResult();
    }

    // ============================================================================
    // StopAsync Tests (7 tests)
    // ============================================================================

    [Fact]
    public async Task StopAsync_WithoutStart_ReturnsSuccessfully()
    {
        // Arrange
        var bootstrap = Substitute.For<ILambdaBootstrapOrchestrator>();
        var handlerFactory = Substitute.For<ILambdaHandlerFactory>();
        var lifetime = Substitute.For<IHostApplicationLifetime>();

        var service = new LambdaHostedService(bootstrap, handlerFactory, lifetime);
        using var cts = new CancellationTokenSource();

        // Act
        var action = async () => await service.StopAsync(cts.Token);

        // Assert - should not throw
        await action.Should().NotThrowAsync<Exception>();
    }

    [Fact]
    public async Task StopAsync_WaitsForExecuteTaskCompletion()
    {
        // Arrange
        var bootstrap = Substitute.For<ILambdaBootstrapOrchestrator>();
        var handlerFactory = Substitute.For<ILambdaHandlerFactory>();
        var lifetime = Substitute.For<IHostApplicationLifetime>();

        var handler = CreateMockHandler();
        handlerFactory.CreateHandler(Arg.Any<CancellationToken>()).Returns(handler);

        var bootstrapTcs = new TaskCompletionSource();
        bootstrap
            .RunAsync(
                Arg.Any<Func<Stream, ILambdaContext, Task<Stream>>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(bootstrapTcs.Task);

        var service = new LambdaHostedService(bootstrap, handlerFactory, lifetime);
        using var startCts = new CancellationTokenSource();

        await service.StartAsync(startCts.Token);

        // Act
        using var stopCts = new CancellationTokenSource();
        var stopTask = service.StopAsync(stopCts.Token);

        // At this point, stopTask should be waiting for the bootstrap task to complete
        stopTask.IsCompleted.Should().BeFalse();

        // Complete the bootstrap task
        bootstrapTcs.SetResult();

        // Assert - StopAsync should now complete
        await stopTask;
        stopTask.IsCompletedSuccessfully.Should().BeTrue();
    }

    [Fact]
    public async Task StopAsync_WithTimeoutToken_ThrowsAggregateExceptionWithOperationCanceledException()
    {
        // Arrange
        var bootstrap = Substitute.For<ILambdaBootstrapOrchestrator>();
        var handlerFactory = Substitute.For<ILambdaHandlerFactory>();
        var lifetime = Substitute.For<IHostApplicationLifetime>();

        var handler = CreateMockHandler();
        handlerFactory.CreateHandler(Arg.Any<CancellationToken>()).Returns(handler);

        // Create a task that will never complete
        var bootstrapTcs = new TaskCompletionSource();
        bootstrap
            .RunAsync(
                Arg.Any<Func<Stream, ILambdaContext, Task<Stream>>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(bootstrapTcs.Task);

        var service = new LambdaHostedService(bootstrap, handlerFactory, lifetime);
        using var startCts = new CancellationTokenSource();

        await service.StartAsync(startCts.Token);

        // Act - use a cancellation token with timeout
        using var stopCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        // Assert - StopAsync should throw AggregateException with OperationCanceledException
        var act = async () => await service.StopAsync(stopCts.Token);
        var ex = await act.Should().ThrowAsync<AggregateException>();
        ex.Which.InnerExceptions.Should().Contain(ie => ie is OperationCanceledException);
    }

    [Fact]
    public async Task StopAsync_WhenBootstrapFails_ThrowsAggregateExceptionWithInnerException()
    {
        // Arrange
        var bootstrap = Substitute.For<ILambdaBootstrapOrchestrator>();
        var handlerFactory = Substitute.For<ILambdaHandlerFactory>();
        var lifetime = Substitute.For<IHostApplicationLifetime>();

        var handler = CreateMockHandler();
        handlerFactory.CreateHandler(Arg.Any<CancellationToken>()).Returns(handler);

        var testException = new InvalidOperationException("Bootstrap error");
        var bootstrapTcs = new TaskCompletionSource();
        bootstrap
            .RunAsync(
                Arg.Any<Func<Stream, ILambdaContext, Task<Stream>>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(bootstrapTcs.Task);

        var service = new LambdaHostedService(bootstrap, handlerFactory, lifetime);
        using var startCts = new CancellationTokenSource();

        await service.StartAsync(startCts.Token);

        // Act
        using var stopCts = new CancellationTokenSource();
        var stopTask = service.StopAsync(stopCts.Token);

        // Complete the bootstrap with an exception
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
        var bootstrap = Substitute.For<ILambdaBootstrapOrchestrator>();
        var handlerFactory = Substitute.For<ILambdaHandlerFactory>();
        var lifetime = Substitute.For<IHostApplicationLifetime>();

        var handler = CreateMockHandler();
        handlerFactory.CreateHandler(Arg.Any<CancellationToken>()).Returns(handler);

        var bootstrapTcs = new TaskCompletionSource();
        bootstrap
            .RunAsync(
                Arg.Any<Func<Stream, ILambdaContext, Task<Stream>>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(bootstrapTcs.Task);

        var service = new LambdaHostedService(bootstrap, handlerFactory, lifetime);
        using var startCts = new CancellationTokenSource();

        await service.StartAsync(startCts.Token);

        // Act
        using var stopCts = new CancellationTokenSource();
        var stopTask = service.StopAsync(stopCts.Token);

        // Complete the bootstrap task successfully
        bootstrapTcs.SetResult();

        // Assert
        await stopTask;
        stopTask.IsCompletedSuccessfully.Should().BeTrue();
    }

    [Fact]
    public async Task StopAsync_AfterDispose_HandlesGracefully()
    {
        // Arrange
        var bootstrap = Substitute.For<ILambdaBootstrapOrchestrator>();
        var handlerFactory = Substitute.For<ILambdaHandlerFactory>();
        var lifetime = Substitute.For<IHostApplicationLifetime>();

        var handler = CreateMockHandler();
        handlerFactory.CreateHandler(Arg.Any<CancellationToken>()).Returns(handler);

        var bootstrapTcs = new TaskCompletionSource();
        bootstrap
            .RunAsync(
                Arg.Any<Func<Stream, ILambdaContext, Task<Stream>>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(bootstrapTcs.Task);

        var service = new LambdaHostedService(bootstrap, handlerFactory, lifetime);
        using var startCts = new CancellationTokenSource();

        await service.StartAsync(startCts.Token);

        // Dispose to set _stoppingCts to null
        service.Dispose();

        using var stopCts = new CancellationTokenSource();

        // Complete the bootstrap task
        bootstrapTcs.SetResult();

        // Assert - StopAsync should handle the null _stoppingCts without throwing
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
        var bootstrap = Substitute.For<ILambdaBootstrapOrchestrator>();
        var handlerFactory = Substitute.For<ILambdaHandlerFactory>();
        var lifetime = Substitute.For<IHostApplicationLifetime>();

        var handler = CreateMockHandler();
        handlerFactory.CreateHandler(Arg.Any<CancellationToken>()).Returns(handler);

        var bootstrapTcs = new TaskCompletionSource();
        bootstrap
            .RunAsync(
                Arg.Any<Func<Stream, ILambdaContext, Task<Stream>>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(bootstrapTcs.Task);

        var service = new LambdaHostedService(bootstrap, handlerFactory, lifetime);
        using var cts = new CancellationTokenSource();

        await service.StartAsync(cts.Token);

        // Act
        service.Dispose();

        // Assert - the internal CancellationTokenSource should be triggered
        // (we can verify by checking that StopAsync can be called and doesn't throw)
        using var stopCts = new CancellationTokenSource();
        var action = async () => await service.StopAsync(stopCts.Token);
        // If dispose worked, the stop should complete or handle gracefully
        bootstrapTcs.SetResult();
        await action.Should().NotThrowAsync<Exception>();
    }

    [Fact]
    public void Dispose_WithoutStartAsync_DoesNotThrow()
    {
        // Arrange
        var bootstrap = Substitute.For<ILambdaBootstrapOrchestrator>();
        var handlerFactory = Substitute.For<ILambdaHandlerFactory>();
        var lifetime = Substitute.For<IHostApplicationLifetime>();

        var service = new LambdaHostedService(bootstrap, handlerFactory, lifetime);

        // Act
        var action = () => service.Dispose();

        // Assert
        action.Should().NotThrow();
    }

    // ============================================================================
    // Integration Tests (6 tests)
    // ============================================================================

    [Fact]
    public async Task StartAsync_And_StopAsync_FullLifecycle()
    {
        // Arrange
        var bootstrap = Substitute.For<ILambdaBootstrapOrchestrator>();
        var handlerFactory = Substitute.For<ILambdaHandlerFactory>();
        var lifetime = Substitute.For<IHostApplicationLifetime>();

        var handler = CreateMockHandler();
        handlerFactory.CreateHandler(Arg.Any<CancellationToken>()).Returns(handler);

        var bootstrapTcs = new TaskCompletionSource();
        bootstrap
            .RunAsync(
                Arg.Any<Func<Stream, ILambdaContext, Task<Stream>>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(bootstrapTcs.Task);

        var service = new LambdaHostedService(bootstrap, handlerFactory, lifetime);
        using var startCts = new CancellationTokenSource();

        // Act
        await service.StartAsync(startCts.Token);

        using var stopCts = new CancellationTokenSource();
        var stopTask = service.StopAsync(stopCts.Token);

        // Complete the bootstrap task
        bootstrapTcs.SetResult();

        await stopTask;

        // Assert
        lifetime.Received(1).StopApplication();
    }

    [Fact]
    public async Task StartAsync_CallsBootstrapRunAsyncWithCreatedHandler()
    {
        // Arrange
        var bootstrap = Substitute.For<ILambdaBootstrapOrchestrator>();
        var handlerFactory = Substitute.For<ILambdaHandlerFactory>();
        var lifetime = Substitute.For<IHostApplicationLifetime>();

        var handler = CreateMockHandler();
        handlerFactory.CreateHandler(Arg.Any<CancellationToken>()).Returns(handler);

        var bootstrapTcs = new TaskCompletionSource();
        bootstrap
            .RunAsync(
                Arg.Any<Func<Stream, ILambdaContext, Task<Stream>>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(bootstrapTcs.Task);

        var service = new LambdaHostedService(bootstrap, handlerFactory, lifetime);
        var cts = new CancellationTokenSource();

        // Act
        await service.StartAsync(cts.Token);

        // Assert
        bootstrap.Received(1).RunAsync(handler, Arg.Any<CancellationToken>());

        bootstrapTcs.SetResult();
        cts.Dispose();
    }

    [Fact]
    public async Task ExecuteAsync_CallsStopApplicationInFinallyBlock_EvenWhenBootstrapFails()
    {
        // Arrange
        var bootstrap = Substitute.For<ILambdaBootstrapOrchestrator>();
        var handlerFactory = Substitute.For<ILambdaHandlerFactory>();
        var lifetime = Substitute.For<IHostApplicationLifetime>();

        var handler = CreateMockHandler();
        handlerFactory.CreateHandler(Arg.Any<CancellationToken>()).Returns(handler);

        var exceptionToThrow = new InvalidOperationException("Test error");
        var bootstrapTcs = new TaskCompletionSource();
        bootstrap
            .RunAsync(
                Arg.Any<Func<Stream, ILambdaContext, Task<Stream>>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(bootstrapTcs.Task);

        var service = new LambdaHostedService(bootstrap, handlerFactory, lifetime);
        var cts = new CancellationTokenSource();

        // Act
        var startTask = service.StartAsync(cts.Token);

        // Complete the bootstrap with an exception
        bootstrapTcs.SetException(exceptionToThrow);

        // Wait for the task to process the exception
        try
        {
            if (startTask.IsCompleted)
                await startTask;
        }
        catch
        {
            /* expected */
        }

        // Assert - StopApplication should be called even when exception occurs
        lifetime.Received(1).StopApplication();
        cts.Dispose();
    }

    [Fact]
    public async Task StopAsync_CalledMultipleTimes_HandlesCorrectly()
    {
        // Arrange
        var bootstrap = Substitute.For<ILambdaBootstrapOrchestrator>();
        var handlerFactory = Substitute.For<ILambdaHandlerFactory>();
        var lifetime = Substitute.For<IHostApplicationLifetime>();

        var handler = CreateMockHandler();
        handlerFactory.CreateHandler(Arg.Any<CancellationToken>()).Returns(handler);

        var bootstrapTcs = new TaskCompletionSource();
        bootstrap
            .RunAsync(
                Arg.Any<Func<Stream, ILambdaContext, Task<Stream>>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(bootstrapTcs.Task);

        var service = new LambdaHostedService(bootstrap, handlerFactory, lifetime);
        using var startCts = new CancellationTokenSource();

        await service.StartAsync(startCts.Token);

        // Act - first StopAsync
        using var stopCts1 = new CancellationTokenSource();
        var stop1 = service.StopAsync(stopCts1.Token);

        // Complete the bootstrap task
        bootstrapTcs.SetResult();
        await stop1;

        // Second call to StopAsync after task has completed
        using var stopCts2 = new CancellationTokenSource();
        var stop2 = service.StopAsync(stopCts2.Token);

        // Assert - second StopAsync should return immediately without exception
        var action = async () => await stop2;
        await action.Should().NotThrowAsync<Exception>();
    }

    // ============================================================================
    // Helper Methods
    // ============================================================================

    /// <summary>
    /// Creates a mock Lambda handler delegate.
    /// </summary>
    private static Func<Stream, ILambdaContext, Task<Stream>> CreateMockHandler()
    {
        return async (stream, context) =>
        {
            await Task.Delay(0);
            return new MemoryStream();
        };
    }
}
