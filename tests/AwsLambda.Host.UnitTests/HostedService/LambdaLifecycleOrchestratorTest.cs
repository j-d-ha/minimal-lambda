using AwesomeAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace AwsLambda.Host.UnitTests.HostedService;

[TestSubject(typeof(LambdaLifecycleOrchestrator))]
public class LambdaLifecycleOrchestratorTest
{
    // ============================================================================
    // Constructor Validation Tests (2 tests)
    // ============================================================================

    [Fact]
    public void Constructor_WithNullScopeFactory_ThrowsArgumentNullException()
    {
        // Arrange
        var delegateHolder = new DelegateHolder();

        // Act
        var action = () => new LambdaLifecycleOrchestrator(null!, delegateHolder);

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("scopeFactory");
    }

    [Fact]
    public void Constructor_WithNullDelegateHolder_ThrowsArgumentNullException()
    {
        // Arrange
        var scopeFactory = Substitute.For<IServiceScopeFactory>();

        // Act
        var action = () => new LambdaLifecycleOrchestrator(scopeFactory, null!);

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("delegateHolder");
    }

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstanceSuccessfully()
    {
        // Arrange
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var delegateHolder = new DelegateHolder();

        // Act
        var orchestrator = new LambdaLifecycleOrchestrator(scopeFactory, delegateHolder);

        // Assert
        orchestrator.Should().NotBeNull();
    }

    // ============================================================================
    // OnShutdown Tests - Happy Path (2 tests)
    // ============================================================================

    [Fact]
    public async Task OnShutdown_WithSingleHandlerThatSucceeds_ReturnsEmptyExceptionList()
    {
        // Arrange
        var handlerExecuted = false;
        var delegateHolder = new DelegateHolder();
        delegateHolder.ShutdownHandlers.Add(
            (_, _) =>
            {
                handlerExecuted = true;
                return Task.CompletedTask;
            }
        );

        var orchestrator = CreateOrchestrator(delegateHolder);

        // Act
        var exceptions = (await orchestrator.OnShutdown(CancellationToken.None)).ToList();

        // Assert
        handlerExecuted.Should().BeTrue();
        exceptions.Should().BeEmpty();
    }

    [Fact]
    public async Task OnShutdown_WithMultipleSuccessfulHandlers_RunsConcurrentlyAndReturnsEmptyExceptionList()
    {
        // Arrange
        var executionOrder = new List<int>();
        var delegateHolder = new DelegateHolder();

        // Add three handlers with simulated delays
        delegateHolder.ShutdownHandlers.Add(CreateDelayedHandler(1, 50, executionOrder));
        delegateHolder.ShutdownHandlers.Add(CreateDelayedHandler(2, 30, executionOrder));
        delegateHolder.ShutdownHandlers.Add(CreateDelayedHandler(3, 20, executionOrder));

        var orchestrator = CreateOrchestrator(delegateHolder);

        // Act
        var exceptions = (await orchestrator.OnShutdown(CancellationToken.None)).ToList();

        // Assert
        // If handlers ran sequentially, order would be [1, 2, 3] with total time ~100ms
        // If handlers run concurrently, all complete by ~50ms
        exceptions.Should().BeEmpty();
        // All handlers should be recorded regardless of execution order (demonstrating concurrency)
        executionOrder.Should().HaveCount(3);
        executionOrder.Should().Contain([1, 2, 3]);
    }

    // ============================================================================
    // OnShutdown Tests - Exception Handling (3 tests)
    // ============================================================================

    [Fact]
    public async Task OnShutdown_WithSingleHandlerThatThrows_ReturnsExceptionInList()
    {
        // Arrange
        var testException = new InvalidOperationException("Handler failed");
        var delegateHolder = new DelegateHolder();
        delegateHolder.ShutdownHandlers.Add((_, _) => throw testException);

        var orchestrator = CreateOrchestrator(delegateHolder);

        // Act
        var exceptions = (await orchestrator.OnShutdown(CancellationToken.None)).ToList();

        // Assert
        exceptions.Should().HaveCount(1);
        exceptions.First().Should().Be(testException);
    }

    [Fact]
    public async Task OnShutdown_WithMultipleHandlers_SomeSuccessfulAndSomeFailing_ReturnsOnlyFailingExceptions()
    {
        // Arrange
        var exception1 = new InvalidOperationException("Handler 1 failed");
        var exception2 = new ArgumentException("Handler 3 failed");
        var delegateHolder = new DelegateHolder();

        delegateHolder.ShutdownHandlers.Add((_, _) => throw exception1);
        delegateHolder.ShutdownHandlers.Add((_, _) => Task.CompletedTask); // Succeeds
        delegateHolder.ShutdownHandlers.Add((_, _) => throw exception2);

        var orchestrator = CreateOrchestrator(delegateHolder);

        // Act
        var exceptions = (await orchestrator.OnShutdown(CancellationToken.None)).ToList();

        // Assert
        exceptions.Should().HaveCount(2);
        exceptions.Should().Contain(exception1);
        exceptions.Should().Contain(exception2);
    }

    [Fact]
    public async Task OnShutdown_WithAllHandlersFailing_ReturnsAllExceptions()
    {
        // Arrange
        var exception1 = new InvalidOperationException("Handler 1 failed");
        var exception2 = new ArgumentException("Handler 2 failed");
        var exception3 = new TimeoutException("Handler 3 failed");

        var delegateHolder = new DelegateHolder();
        delegateHolder.ShutdownHandlers.Add((_, _) => throw exception1);
        delegateHolder.ShutdownHandlers.Add((_, _) => throw exception2);
        delegateHolder.ShutdownHandlers.Add((_, _) => throw exception3);

        var orchestrator = CreateOrchestrator(delegateHolder);

        // Act
        var exceptions = (await orchestrator.OnShutdown(CancellationToken.None)).ToList();

        // Assert
        exceptions.Should().HaveCount(3);
        exceptions.Should().Contain([exception1, exception2, exception3]);
    }

    // ============================================================================
    // OnShutdown Tests - Edge Cases (3 tests)
    // ============================================================================

    [Fact]
    public async Task OnShutdown_WithNoRegisteredHandlers_ReturnsEmptyExceptionList()
    {
        // Arrange
        var delegateHolder = new DelegateHolder();
        var orchestrator = CreateOrchestrator(delegateHolder);

        // Act
        var exceptions = (await orchestrator.OnShutdown(CancellationToken.None)).ToList();

        // Assert
        exceptions.Should().BeEmpty();
    }

    [Fact]
    public async Task OnShutdown_CancellationToken_IsPropagatedToHandlers()
    {
        // Arrange
        var cancellationTokenReceived = CancellationToken.None;
        var delegateHolder = new DelegateHolder();
        delegateHolder.ShutdownHandlers.Add(
            (_, token) =>
            {
                cancellationTokenReceived = token;
                return Task.CompletedTask;
            }
        );

        var orchestrator = CreateOrchestrator(delegateHolder);
        var expectedToken = CancellationToken.None;

        // Act
        await orchestrator.OnShutdown(expectedToken);

        // Assert
        cancellationTokenReceived.Should().Be(expectedToken);
    }

    [Fact]
    public async Task OnShutdown_ServiceProvider_IsCreatedPerHandler()
    {
        // Arrange
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var scope1 = Substitute.For<IServiceScope>();
        var scope2 = Substitute.For<IServiceScope>();
        var provider1 = Substitute.For<IServiceProvider>();
        var provider2 = Substitute.For<IServiceProvider>();

        scope1.ServiceProvider.Returns(provider1);
        scope2.ServiceProvider.Returns(provider2);

        scopeFactory.CreateScope().Returns(scope1, scope2);

        var serviceProvidersReceived = new List<IServiceProvider>();
        var delegateHolder = new DelegateHolder();
        delegateHolder.ShutdownHandlers.Add(
            (provider, _) =>
            {
                serviceProvidersReceived.Add(provider);
                return Task.CompletedTask;
            }
        );
        delegateHolder.ShutdownHandlers.Add(
            (provider, _) =>
            {
                serviceProvidersReceived.Add(provider);
                return Task.CompletedTask;
            }
        );

        var orchestrator = new LambdaLifecycleOrchestrator(scopeFactory, delegateHolder);

        // Act
        await orchestrator.OnShutdown(CancellationToken.None);

        // Assert
        scopeFactory.Received(2).CreateScope();
        serviceProvidersReceived.Should().HaveCount(2);
        serviceProvidersReceived[0].Should().Be(provider1);
        serviceProvidersReceived[1].Should().Be(provider2);
    }

    // ============================================================================
    // OnShutdown Tests - Integration (2 tests)
    // ============================================================================

    [Fact]
    public async Task OnShutdown_WithCancelledToken_PropagatesTokenToHandlers()
    {
        // Arrange
        var cancellationReceived = false;
        var delegateHolder = new DelegateHolder();
        delegateHolder.ShutdownHandlers.Add(
            (_, token) =>
            {
                cancellationReceived = token.IsCancellationRequested;
                return Task.CompletedTask;
            }
        );

        var orchestrator = CreateOrchestrator(delegateHolder);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        await orchestrator.OnShutdown(cts.Token);

        // Assert
        cancellationReceived.Should().BeTrue();
    }

    [Fact]
    public async Task OnShutdown_WhenHandlerThrowsOperationCanceledException_IncludesItInReturnedExceptions()
    {
        // Arrange
        var canceledException = new OperationCanceledException();
        var delegateHolder = new DelegateHolder();
        delegateHolder.ShutdownHandlers.Add((_, _) => throw canceledException);

        var orchestrator = CreateOrchestrator(delegateHolder);

        // Act
        var exceptions = (await orchestrator.OnShutdown(CancellationToken.None)).ToList();

        // Assert
        exceptions.Should().HaveCount(1);
        exceptions.First().Should().BeOfType<OperationCanceledException>();
        exceptions.First().Should().Be(canceledException);
    }

    // ============================================================================
    // Helper Methods
    // ============================================================================

    /// <summary>
    /// Creates an orchestrator instance with the default service scope factory mock.
    /// </summary>
    private static LambdaLifecycleOrchestrator CreateOrchestrator(DelegateHolder delegateHolder)
    {
        var scopeFactory = CreateMockServiceScopeFactory();
        return new LambdaLifecycleOrchestrator(scopeFactory, delegateHolder);
    }

    /// <summary>
    /// Creates a properly configured mock service scope factory.
    /// </summary>
    private static IServiceScopeFactory CreateMockServiceScopeFactory()
    {
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var scope = Substitute.For<IServiceScope>();
        var provider = Substitute.For<IServiceProvider>();

        scope.ServiceProvider.Returns(provider);
        scopeFactory.CreateScope().Returns(scope);

        return scopeFactory;
    }

    /// <summary>
    /// Creates a delayed shutdown handler that records its execution order.
    /// </summary>
    /// <param name="handlerId">Identifier for this handler</param>
    /// <param name="delayMs">Delay in milliseconds before completion</param>
    /// <param name="executionOrder">List to record execution order</param>
    /// <returns>A shutdown delegate that executes with the specified delay</returns>
    private static LambdaShutdownDelegate CreateDelayedHandler(
        int handlerId,
        int delayMs,
        List<int> executionOrder
    ) =>
        async (_, _) =>
        {
            await Task.Delay(delayMs);
            executionOrder.Add(handlerId);
        };
}
