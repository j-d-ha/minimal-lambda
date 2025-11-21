using System.Reflection;
using Amazon.Lambda.Core;
using AwesomeAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace AwsLambda.Host.UnitTests.Core.Runtime;

[TestSubject(typeof(LambdaHostedService))]
public class LambdaHostedServiceTests
{
    private readonly Fixture _fixture = new();

    /// <summary>Fixture for setting up mocks and dependencies for LambdaHostedService tests.</summary>
    private class Fixture
    {
        public Fixture()
        {
            Bootstrap = Substitute.For<ILambdaBootstrapOrchestrator>();
            HandlerFactory = Substitute.For<ILambdaHandlerFactory>();
            Lifetime = Substitute.For<IHostApplicationLifetime>();
            OnInitBuilderFactory = Substitute.For<IOnInitBuilderFactory>();
            OnShutdownBuilderFactory = Substitute.For<IOnShutdownBuilderFactory>();
            OnInitBuilder = Substitute.For<ILambdaOnInitBuilder>();
            OnShutdownBuilder = Substitute.For<ILambdaOnShutdownBuilder>();

            LambdaHostOptions = Microsoft.Extensions.Options.Options.Create(
                new LambdaHostedServiceOptions()
            );

            BootstrapTask = Task.CompletedTask;
            OnShutdownBuilderHandler = async ct => await Task.CompletedTask;

            SetupDefaults();
        }

        public ILambdaBootstrapOrchestrator Bootstrap { get; }

        public Task BootstrapTask { get; set; }
        public ILambdaHandlerFactory HandlerFactory { get; }
        public IOptions<LambdaHostedServiceOptions> LambdaHostOptions { get; }
        public IHostApplicationLifetime Lifetime { get; }
        public ILambdaOnInitBuilder OnInitBuilder { get; }
        public IOnInitBuilderFactory OnInitBuilderFactory { get; }
        public ILambdaOnShutdownBuilder OnShutdownBuilder { get; }
        public IOnShutdownBuilderFactory OnShutdownBuilderFactory { get; }
        public Func<CancellationToken, Task>? OnShutdownBuilderHandler { get; set; }

        private void SetupDefaults()
        {
            HandlerFactory
                .CreateHandler(Arg.Any<CancellationToken>())
                .Returns(_ => CreateDefaultHandler());

            OnInitBuilderFactory.CreateBuilder().Returns(OnInitBuilder);

            OnInitBuilder.Build().Returns(async ct => await Task.FromResult(true));

            OnShutdownBuilderFactory.CreateBuilder().Returns(OnShutdownBuilder);

            OnShutdownBuilder
                .Build()
                .Returns(ct => OnShutdownBuilderHandler?.Invoke(ct) ?? Task.CompletedTask);

            Bootstrap
                .RunAsync(
                    Arg.Any<Func<Stream, ILambdaContext, Task<Stream>>>(),
                    Arg.Any<Func<CancellationToken, Task<bool>>>(),
                    Arg.Any<CancellationToken>()
                )
                .Returns(_ => BootstrapTask);

            Lifetime.WhenForAnyArgs(l => l.StopApplication()).Do(c => { });
        }

        public LambdaHostedService CreateService() =>
            new(
                Bootstrap,
                HandlerFactory,
                Lifetime,
                OnInitBuilderFactory,
                LambdaHostOptions,
                OnShutdownBuilderFactory
            );

        private static Func<Stream, ILambdaContext, Task<Stream>> CreateDefaultHandler() =>
            async (inputStream, context) =>
            {
                var responseStream = new MemoryStream();
                await Task.CompletedTask;
                return responseStream;
            };
    }

    #region Constructor Validation Tests

    [Theory]
    [InlineData(0)] // bootstrap
    [InlineData(1)] // handlerFactory
    [InlineData(2)] // lifetime
    [InlineData(3)] // onInitBuilderFactory
    [InlineData(4)] // lambdaHostOptions
    [InlineData(5)] // onShutdownBuilderFactory
    public void Constructor_WithNullParameter_ThrowsArgumentNullException(int parameterIndex)
    {
        // Arrange
        var bootstrap = parameterIndex == 0 ? null : _fixture.Bootstrap;
        var handlerFactory = parameterIndex == 1 ? null : _fixture.HandlerFactory;
        var lifetime = parameterIndex == 2 ? null : _fixture.Lifetime;
        var onInitBuilderFactory = parameterIndex == 3 ? null : _fixture.OnInitBuilderFactory;
        var lambdaHostOptions = parameterIndex == 4 ? null : _fixture.LambdaHostOptions;
        var onShutdownBuilderFactory =
            parameterIndex == 5 ? null : _fixture.OnShutdownBuilderFactory;

        // Act & Assert
        var act = () =>
            new LambdaHostedService(
                bootstrap!,
                handlerFactory!,
                lifetime!,
                onInitBuilderFactory!,
                lambdaHostOptions!,
                onShutdownBuilderFactory!
            );
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithValidParameters_SuccessfullyConstructs()
    {
        // Act
        var service = _fixture.CreateService();

        // Assert
        service.Should().NotBeNull();
        service.Should().BeAssignableTo<IHostedService>();
        service.Should().BeAssignableTo<IDisposable>();
    }

    #endregion

    #region StartAsync Tests

    [Fact]
    public async Task StartAsync_CreatesLinkedCancellationTokenSource()
    {
        // Arrange
        var service = _fixture.CreateService();
        using var cts = new CancellationTokenSource();

        // Act
        await service.StartAsync(cts.Token);

        // Assert
        // Verify by attempting to cancel and checking if the bootstrap was called with a token
        await _fixture
            .Bootstrap.Received(1)
            .RunAsync(
                Arg.Any<Func<Stream, ILambdaContext, Task<Stream>>>(),
                Arg.Any<Func<CancellationToken, Task<bool>>>(),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task StartAsync_CreatesRequestHandler()
    {
        // Arrange
        var service = _fixture.CreateService();

        // Act
        await service.StartAsync(CancellationToken.None);

        // Assert
        _fixture.HandlerFactory.Received(1).CreateHandler(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartAsync_CreatesOnInitBuilder()
    {
        // Arrange
        var service = _fixture.CreateService();

        // Act
        await service.StartAsync(CancellationToken.None);

        // Assert
        _fixture.OnInitBuilderFactory.Received(1).CreateBuilder();
    }

    [Fact]
    public async Task StartAsync_InvokesConfigureOnInitBuilder_WhenProvided()
    {
        // Arrange
        var configureInvoked = false;
        Action<ILambdaOnInitBuilder>? configureAction = builder =>
        {
            configureInvoked = true;
        };

        var lambdaOptions = new LambdaHostedServiceOptions
        {
            ConfigureOnInitBuilder = configureAction,
        };
        var options = Microsoft.Extensions.Options.Options.Create(lambdaOptions);

        var service = new LambdaHostedService(
            _fixture.Bootstrap,
            _fixture.HandlerFactory,
            _fixture.Lifetime,
            _fixture.OnInitBuilderFactory,
            options,
            _fixture.OnShutdownBuilderFactory
        );

        // Act
        await service.StartAsync(CancellationToken.None);

        // Assert
        configureInvoked.Should().BeTrue();
    }

    [Fact]
    public async Task StartAsync_CreatesOnShutdownBuilder()
    {
        // Arrange
        var service = _fixture.CreateService();

        // Act
        await service.StartAsync(CancellationToken.None);

        // Assert
        _fixture.OnShutdownBuilderFactory.Received(1).CreateBuilder();
    }

    [Fact]
    public async Task StartAsync_InvokesConfigureOnShutdownBuilder_WhenProvided()
    {
        // Arrange
        var configureInvoked = false;
        Action<ILambdaOnShutdownBuilder>? configureAction = builder =>
        {
            configureInvoked = true;
        };

        var lambdaOptions = new LambdaHostedServiceOptions
        {
            ConfigureOnShutdownBuilder = configureAction,
        };
        var options = Microsoft.Extensions.Options.Options.Create(lambdaOptions);

        var service = new LambdaHostedService(
            _fixture.Bootstrap,
            _fixture.HandlerFactory,
            _fixture.Lifetime,
            _fixture.OnInitBuilderFactory,
            options,
            _fixture.OnShutdownBuilderFactory
        );

        // Act
        await service.StartAsync(CancellationToken.None);

        // Assert
        configureInvoked.Should().BeTrue();
    }

    [Fact]
    public async Task StartAsync_StartsExecuteAsyncTask()
    {
        // Arrange
        var service = _fixture.CreateService();
        _fixture.BootstrapTask = new TaskCompletionSource().Task;

        // Act
        await service.StartAsync(CancellationToken.None);

        // Assert
        _fixture.HandlerFactory.Received(1).CreateHandler(Arg.Any<CancellationToken>());
    }

    #endregion

    #region StopAsync Tests

    [Fact]
    public async Task StopAsync_WithoutStartAsync_ReturnsImmediately()
    {
        // Arrange
        var service = _fixture.CreateService();

        // Act & Assert (should not throw)
        await service.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task StopAsync_CancelsExecuteTask()
    {
        // Arrange
        var taskCompletionSource = new TaskCompletionSource();
        _fixture.BootstrapTask = taskCompletionSource.Task;

        var service = _fixture.CreateService();
        await service.StartAsync(CancellationToken.None);

        // Act
        var stopTask = service.StopAsync(CancellationToken.None);
        taskCompletionSource.SetResult();

        await stopTask;

        // Assert
        _fixture
            .Bootstrap.Received(1)
            .RunAsync(
                Arg.Any<Func<Stream, ILambdaContext, Task<Stream>>>(),
                Arg.Any<Func<CancellationToken, Task<bool>>>(),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task StopAsync_WaitsForBootstrapToComplete()
    {
        // Arrange
        var taskCompletionSource = new TaskCompletionSource();
        _fixture.BootstrapTask = taskCompletionSource.Task;

        var service = _fixture.CreateService();
        await service.StartAsync(CancellationToken.None);

        // Verify task is not completed
        service.GetExecuteTask().IsCompleted.Should().BeFalse();

        // Act
        var stopTask = service.StopAsync(CancellationToken.None);
        taskCompletionSource.SetResult();

        // Assert
        var act = async () => await stopTask;
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task StopAsync_ThrowsOperationCanceledException_WhenBootstrapTimesOut()
    {
        // Arrange
        var taskCompletionSource = new TaskCompletionSource();
        _fixture.BootstrapTask = taskCompletionSource.Task;

        var service = _fixture.CreateService();
        await service.StartAsync(CancellationToken.None);

        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(10));

        // Act & Assert
        var act = () => service.StopAsync(timeoutCts.Token);
        await act.Should()
            .ThrowExactlyAsync<AggregateException>()
            .Where(ae =>
                ae.InnerExceptions.Any(ex =>
                    ex is OperationCanceledException && ex.Message.Contains("Graceful shutdown")
                )
            );
    }

    [Fact]
    public async Task StopAsync_InvokesShutdownHandler()
    {
        // Arrange
        var taskCompletionSource = new TaskCompletionSource();
        _fixture.BootstrapTask = taskCompletionSource.Task;

        var shutdownHandlerInvoked = false;
        _fixture.OnShutdownBuilderHandler = async ct =>
        {
            shutdownHandlerInvoked = true;
            await Task.CompletedTask;
        };

        var service = _fixture.CreateService();
        await service.StartAsync(CancellationToken.None);

        // Act
        var stopTask = service.StopAsync(CancellationToken.None);
        taskCompletionSource.SetResult();
        await stopTask;

        // Assert
        shutdownHandlerInvoked.Should().BeTrue();
    }

    [Fact]
    public async Task StopAsync_PropagatesShutdownHandlerException()
    {
        // Arrange
        var shutdownException = new InvalidOperationException("Shutdown handler error");
        var taskCompletionSource = new TaskCompletionSource();
        _fixture.BootstrapTask = taskCompletionSource.Task;

        _fixture.OnShutdownBuilderHandler = async ct =>
        {
            await Task.CompletedTask;
            throw shutdownException;
        };

        var service = _fixture.CreateService();
        await service.StartAsync(CancellationToken.None);

        // Act & Assert - StopAsync should propagate exception from shutdown handler
        var stopTask = service.StopAsync(CancellationToken.None);
        taskCompletionSource.SetResult();

        var act = async () => await stopTask;
        await act.Should()
            .ThrowExactlyAsync<AggregateException>()
            .Where(ae => ae.InnerExceptions.Contains(shutdownException));
    }

    [Fact]
    public async Task StopAsync_PropagatesNonCanceledExceptionFromExecuteTask()
    {
        // Arrange
        var handlerException = new InvalidOperationException("Handler error");
        var taskCompletionSource = new TaskCompletionSource();
        _fixture.BootstrapTask = taskCompletionSource.Task;

        var service = _fixture.CreateService();
        await service.StartAsync(CancellationToken.None);

        // Simulate an exception from the execute task (after StartAsync completes)
        taskCompletionSource.SetException(handlerException);

        // Act & Assert - StopAsync should propagate non-TaskCanceledException from execute task
        var act = () => service.StopAsync(CancellationToken.None);
        await act.Should()
            .ThrowExactlyAsync<AggregateException>()
            .Where(ae => ae.InnerExceptions.Contains(handlerException));
    }

    [Fact]
    public async Task StopAsync_PropagatesBootstrapExceptionWhenBootstrapFails()
    {
        // Arrange
        var bootstrapException = new InvalidOperationException("Bootstrap failed");
        var taskCompletionSource = new TaskCompletionSource();
        taskCompletionSource.SetException(bootstrapException);
        _fixture.BootstrapTask = taskCompletionSource.Task;

        var service = _fixture.CreateService();

        // Act & Assert - StartAsync returns the failed task which throws when awaited
        var act = async () => await service.StartAsync(CancellationToken.None);
        await act.Should().ThrowExactlyAsync<InvalidOperationException>();
    }

    #endregion

    #region Dispose Tests

    [Fact]
    public async Task Dispose_Idempotent_WhenNotStarted()
    {
        // Arrange
        var service = _fixture.CreateService();

        // Act & Assert - multiple dispose calls should not throw
        service.Dispose();
        service.Dispose();
        service.Dispose();
    }

    [Fact]
    public void Dispose_IsIdempotent()
    {
        // Arrange
        var service = _fixture.CreateService();

        // Act & Assert (should not throw)
        service.Dispose();
        service.Dispose();
        service.Dispose();
    }

    [Fact]
    public async Task Dispose_DisposesExecuteTask()
    {
        // Arrange
        var taskCompletionSource = new TaskCompletionSource();
        _fixture.BootstrapTask = taskCompletionSource.Task;

        var service = _fixture.CreateService();
        await service.StartAsync(CancellationToken.None);

        // Act
        taskCompletionSource.SetResult();
        await Task.Delay(10); // Allow task to complete
        service.Dispose();

        // Assert
        service.GetExecuteTask().IsCompleted.Should().BeTrue();
    }

    #endregion

    #region ExecuteAsync Behavior Tests (via StartAsync/StopAsync)

    [Fact]
    public async Task ExecuteAsync_CallsBootstrapRunAsync()
    {
        // Arrange
        var service = _fixture.CreateService();

        // Act
        await service.StartAsync(CancellationToken.None);

        // Assert
        _fixture
            .Bootstrap.Received(1)
            .RunAsync(
                Arg.Any<Func<Stream, ILambdaContext, Task<Stream>>>(),
                Arg.Any<Func<CancellationToken, Task<bool>>>(),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task ExecuteAsync_CallsLifetimeStopApplication_InFinallyBlock()
    {
        // Arrange
        var taskCompletionSource = new TaskCompletionSource();
        _fixture.BootstrapTask = taskCompletionSource.Task;

        var service = _fixture.CreateService();
        await service.StartAsync(CancellationToken.None);

        // Act
        taskCompletionSource.SetResult();

        // Wait a bit for the finally block to execute
        await Task.Delay(50);

        // Assert
        _fixture.Lifetime.Received(1).StopApplication();
    }

    #endregion

    #region Full Lifecycle Tests

    [Fact]
    public async Task FullLifecycle_StartAsyncStopAsyncDispose()
    {
        // Arrange
        var taskCompletionSource = new TaskCompletionSource();
        _fixture.BootstrapTask = taskCompletionSource.Task;

        var service = _fixture.CreateService();

        // Act - StartAsync
        await service.StartAsync(CancellationToken.None);
        service.GetExecuteTask().Should().NotBeNull();

        // Act - StopAsync
        var stopTask = service.StopAsync(CancellationToken.None);
        taskCompletionSource.SetResult();
        await stopTask;

        // Act - Dispose
        service.Dispose();

        // Assert - all phases completed without error
        service.Should().NotBeNull();
    }

    [Fact]
    public async Task FullLifecycle_WithException_PropagatesInStartAsync()
    {
        // Arrange
        var bootstrapException = new InvalidOperationException("Bootstrap error");
        var taskCompletionSource = new TaskCompletionSource();
        taskCompletionSource.SetException(bootstrapException);
        _fixture.BootstrapTask = taskCompletionSource.Task;

        var service = _fixture.CreateService();

        // Act & Assert - StartAsync returns the failed task which throws when awaited
        var act = async () => await service.StartAsync(CancellationToken.None);
        await act.Should().ThrowExactlyAsync<InvalidOperationException>();
    }

    #endregion
}

/// <summary>Extension methods for testing LambdaHostedService private state.</summary>
internal static class LambdaHostedServiceTestExtensions
{
    public static Task? GetExecuteTask(this LambdaHostedService service)
    {
        var field = typeof(LambdaHostedService).GetField(
            "_executeTask",
            BindingFlags.NonPublic | BindingFlags.Instance
        );
        return (Task?)field?.GetValue(service);
    }
}
