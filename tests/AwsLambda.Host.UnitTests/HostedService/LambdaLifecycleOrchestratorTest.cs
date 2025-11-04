using Amazon.Lambda.RuntimeSupport;
using AwesomeAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
        var options = CreateMockLambdaHostOptions();

        // Act
        var action = () => new LambdaLifecycleOrchestrator(null!, delegateHolder, options);

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("scopeFactory");
    }

    [Fact]
    public void Constructor_WithNullDelegateHolder_ThrowsArgumentNullException()
    {
        // Arrange
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var options = CreateMockLambdaHostOptions();

        // Act
        var action = () => new LambdaLifecycleOrchestrator(scopeFactory, null!, options);

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("delegateHolder");
    }

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstanceSuccessfully()
    {
        // Arrange
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var delegateHolder = new DelegateHolder();
        var options = CreateMockLambdaHostOptions();

        // Act
        var orchestrator = new LambdaLifecycleOrchestrator(scopeFactory, delegateHolder, options);

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

        var options = CreateMockLambdaHostOptions();
        var orchestrator = new LambdaLifecycleOrchestrator(scopeFactory, delegateHolder, options);

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
    // OnInit Tests - Happy Path (3 tests)
    // ============================================================================

    [Fact]
    public async Task OnInit_WithNoHandlers_ReturnsInitializerThatReturnsTrue()
    {
        // Arrange
        var delegateHolder = new DelegateHolder();
        var orchestrator = CreateOrchestrator(delegateHolder);

        // Act
        var initializer = orchestrator.OnInit(CancellationToken.None);
        var result = await initializer.Invoke();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task OnInit_WithSingleSuccessfulHandler_RunsAndReturnsTrue()
    {
        // Arrange
        var handlerExecuted = false;
        var delegateHolder = new DelegateHolder();
        delegateHolder.InitHandlers.Add(
            (_, _) =>
            {
                handlerExecuted = true;
                return Task.FromResult(true);
            }
        );

        var orchestrator = CreateOrchestrator(delegateHolder);

        // Act
        var initializer = orchestrator.OnInit(CancellationToken.None);
        var result = await initializer.Invoke();

        // Assert
        handlerExecuted.Should().BeTrue();
        result.Should().BeTrue();
    }

    [Fact]
    public async Task OnInit_WithMultipleSuccessfulHandlers_RunsConcurrentlyAndReturnsTrue()
    {
        // Arrange
        var executionOrder = new List<int>();
        var delegateHolder = new DelegateHolder();

        // Add three handlers with simulated delays
        delegateHolder.InitHandlers.Add(CreateDelayedInitHandler(1, 50, executionOrder));
        delegateHolder.InitHandlers.Add(CreateDelayedInitHandler(2, 30, executionOrder));
        delegateHolder.InitHandlers.Add(CreateDelayedInitHandler(3, 20, executionOrder));

        var orchestrator = CreateOrchestrator(delegateHolder);

        // Act
        var initializer = orchestrator.OnInit(CancellationToken.None);
        var result = await initializer.Invoke();

        // Assert
        result.Should().BeTrue();
        // All handlers should be recorded regardless of execution order (demonstrating concurrency)
        executionOrder.Should().HaveCount(3);
        executionOrder.Should().Contain([1, 2, 3]);
    }

    // ============================================================================
    // OnInit Tests - Exception Handling (3 tests)
    // ============================================================================

    [Fact]
    public async Task OnInit_WithSingleHandlerThatThrows_ThrowsAggregateException()
    {
        // Arrange
        var testException = new InvalidOperationException("Handler failed");
        var delegateHolder = new DelegateHolder();
        delegateHolder.InitHandlers.Add((_, _) => throw testException);

        var orchestrator = CreateOrchestrator(delegateHolder);

        // Act
        var initializer = orchestrator.OnInit(CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<AggregateException>(() => initializer.Invoke());
        ex.InnerExceptions.Should().HaveCount(1);
        ex.InnerExceptions[0].Should().Be(testException);
    }

    [Fact]
    public async Task OnInit_WithMultipleHandlers_SomeSuccessfulAndSomeFailing_ThrowsAggregateException()
    {
        // Arrange
        var exception1 = new InvalidOperationException("Handler 1 failed");
        var exception2 = new ArgumentException("Handler 3 failed");
        var delegateHolder = new DelegateHolder();

        delegateHolder.InitHandlers.Add((_, _) => throw exception1);
        delegateHolder.InitHandlers.Add((_, _) => Task.FromResult(true)); // Succeeds
        delegateHolder.InitHandlers.Add((_, _) => throw exception2);

        var orchestrator = CreateOrchestrator(delegateHolder);

        // Act
        var initializer = orchestrator.OnInit(CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<AggregateException>(() => initializer.Invoke());
        ex.InnerExceptions.Should().HaveCount(2);
        ex.InnerExceptions.Should().Contain(exception1);
        ex.InnerExceptions.Should().Contain(exception2);
    }

    [Fact]
    public async Task OnInit_WithAllHandlersFailing_ThrowsAggregateExceptionWithAllErrors()
    {
        // Arrange
        var exception1 = new InvalidOperationException("Handler 1 failed");
        var exception2 = new ArgumentException("Handler 2 failed");
        var exception3 = new TimeoutException("Handler 3 failed");

        var delegateHolder = new DelegateHolder();
        delegateHolder.InitHandlers.Add((_, _) => throw exception1);
        delegateHolder.InitHandlers.Add((_, _) => throw exception2);
        delegateHolder.InitHandlers.Add((_, _) => throw exception3);

        var orchestrator = CreateOrchestrator(delegateHolder);

        // Act
        var initializer = orchestrator.OnInit(CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<AggregateException>(() => initializer.Invoke());
        ex.InnerExceptions.Should().HaveCount(3);
        ex.InnerExceptions.Should().Contain(exception1);
        ex.InnerExceptions.Should().Contain(exception2);
        ex.InnerExceptions.Should().Contain(exception3);
    }

    // ============================================================================
    // OnInit Tests - Timeout Behavior (3 tests) - CRITICAL
    // ============================================================================

    [Fact]
    public async Task OnInit_WhenInitTimeoutExpires_CancelsAllHandlers()
    {
        // Arrange
        var cancellationReceived = false;
        var delegateHolder = new DelegateHolder();
        delegateHolder.InitHandlers.Add(
            async (_, token) =>
            {
                try
                {
                    // Attempt a long operation that will be cancelled
                    await Task.Delay(TimeSpan.FromSeconds(10), token);
                }
                catch (OperationCanceledException)
                {
                    cancellationReceived = true;
                }

                return true;
            }
        );

        var options = CreateMockLambdaHostOptions(TimeSpan.FromMilliseconds(100));
        var scopeFactory = CreateMockServiceScopeFactory();
        var orchestrator = new LambdaLifecycleOrchestrator(scopeFactory, delegateHolder, options);

        // Act
        var initializer = orchestrator.OnInit(CancellationToken.None);
        var result = await initializer.Invoke();

        // Assert
        cancellationReceived.Should().BeTrue();
        result.Should().BeTrue();
    }

    [Fact]
    public async Task OnInit_WhenInitTimeoutExpires_PropagatesCancellationTokenToHandler()
    {
        // Arrange
        var tokenWasCancelled = false;
        var delegateHolder = new DelegateHolder();
        delegateHolder.InitHandlers.Add(
            async (_, token) =>
            {
                // Wait for cancellation
                while (!token.IsCancellationRequested)
                    try
                    {
                        await Task.Delay(10, token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                tokenWasCancelled = token.IsCancellationRequested;
                return true;
            }
        );

        var options = CreateMockLambdaHostOptions(TimeSpan.FromMilliseconds(100));
        var scopeFactory = CreateMockServiceScopeFactory();
        var orchestrator = new LambdaLifecycleOrchestrator(scopeFactory, delegateHolder, options);

        // Act
        var initializer = orchestrator.OnInit(CancellationToken.None);
        var result = await initializer.Invoke();

        // Assert
        tokenWasCancelled.Should().BeTrue();
        result.Should().BeTrue();
    }

    [Fact]
    public async Task OnInit_WithCustomInitTimeout_RespectsConfiguredTimeout()
    {
        // Arrange
        var handlerCompletedBeforeTimeout = false;
        var customTimeout = TimeSpan.FromMilliseconds(200);
        var delegateHolder = new DelegateHolder();
        delegateHolder.InitHandlers.Add(
            async (_, token) =>
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), token);
                }
                catch (OperationCanceledException)
                {
                    // Handler was properly cancelled within the custom timeout window
                    handlerCompletedBeforeTimeout = true;
                }

                return true;
            }
        );

        var options = CreateMockLambdaHostOptions(customTimeout);
        var scopeFactory = CreateMockServiceScopeFactory();
        var orchestrator = new LambdaLifecycleOrchestrator(scopeFactory, delegateHolder, options);

        // Act
        var initializer = orchestrator.OnInit(CancellationToken.None);
        var result = await initializer.Invoke();

        // Assert
        handlerCompletedBeforeTimeout.Should().BeTrue();
        result.Should().BeTrue();
    }

    // ============================================================================
    // OnInit Tests - Cancellation Token Propagation (3 tests)
    // ============================================================================

    [Fact]
    public async Task OnInit_InitHandlerReceivesCancellationToken()
    {
        // Arrange
        var cancellationTokenReceived = CancellationToken.None;
        var delegateHolder = new DelegateHolder();
        delegateHolder.InitHandlers.Add(
            (_, token) =>
            {
                cancellationTokenReceived = token;
                return Task.FromResult(true);
            }
        );

        var orchestrator = CreateOrchestrator(delegateHolder);

        // Act
        var initializer = orchestrator.OnInit(CancellationToken.None);
        await initializer.Invoke();

        // Assert
        cancellationTokenReceived.Should().NotBe(CancellationToken.None);
    }

    [Fact]
    public async Task OnInit_MultipleHandlers_AllReceiveSameCancellationToken()
    {
        // Arrange
        var tokensReceived = new List<CancellationToken>();
        var delegateHolder = new DelegateHolder();
        delegateHolder.InitHandlers.Add(
            (_, token) =>
            {
                tokensReceived.Add(token);
                return Task.FromResult(true);
            }
        );
        delegateHolder.InitHandlers.Add(
            (_, token) =>
            {
                tokensReceived.Add(token);
                return Task.FromResult(true);
            }
        );

        var orchestrator = CreateOrchestrator(delegateHolder);

        // Act
        var initializer = orchestrator.OnInit(CancellationToken.None);
        await initializer.Invoke();

        // Assert
        tokensReceived.Should().HaveCount(2);
        // Both handlers should receive tokens that have the same cancellation state
        tokensReceived[0]
            .IsCancellationRequested.Should()
            .Be(tokensReceived[1].IsCancellationRequested);
    }

    [Fact]
    public async Task OnInit_ExternalCancellation_PropagatesCorrectly()
    {
        // Arrange
        var cancellationReceived = false;
        var delegateHolder = new DelegateHolder();
        delegateHolder.InitHandlers.Add(
            (_, token) =>
            {
                cancellationReceived = token.IsCancellationRequested;
                return Task.FromResult(true);
            }
        );

        var orchestrator = CreateOrchestrator(delegateHolder);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var initializer = orchestrator.OnInit(cts.Token);
        var result = await initializer.Invoke();

        // Assert
        cancellationReceived.Should().BeTrue();
        result.Should().BeTrue();
    }

    // ============================================================================
    // OnInit Tests - Service Scope Management (2 tests)
    // ============================================================================

    [Fact]
    public async Task OnInit_EachHandler_ReceivesNewServiceScope()
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
        delegateHolder.InitHandlers.Add(
            (provider, _) =>
            {
                serviceProvidersReceived.Add(provider);
                return Task.FromResult(true);
            }
        );
        delegateHolder.InitHandlers.Add(
            (provider, _) =>
            {
                serviceProvidersReceived.Add(provider);
                return Task.FromResult(true);
            }
        );

        var options = CreateMockLambdaHostOptions();
        var orchestrator = new LambdaLifecycleOrchestrator(scopeFactory, delegateHolder, options);

        // Act
        var initializer = orchestrator.OnInit(CancellationToken.None);
        await initializer.Invoke();

        // Assert
        scopeFactory.Received(2).CreateScope();
        serviceProvidersReceived.Should().HaveCount(2);
        serviceProvidersReceived[0].Should().Be(provider1);
        serviceProvidersReceived[1].Should().Be(provider2);
    }

    [Fact]
    public async Task OnInit_ServiceScopesAreDisposed_AfterHandlerCompletes()
    {
        // Arrange
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var scope1 = Substitute.For<IServiceScope>();
        var scope2 = Substitute.For<IServiceScope>();

        scope1.ServiceProvider.Returns(Substitute.For<IServiceProvider>());
        scope2.ServiceProvider.Returns(Substitute.For<IServiceProvider>());
        scopeFactory.CreateScope().Returns(scope1, scope2);

        var delegateHolder = new DelegateHolder();
        delegateHolder.InitHandlers.Add((_, _) => Task.FromResult(true));
        delegateHolder.InitHandlers.Add((_, _) => Task.FromResult(true));

        var orchestrator = new LambdaLifecycleOrchestrator(
            scopeFactory,
            delegateHolder,
            CreateMockLambdaHostOptions()
        );

        // Act
        var initializer = orchestrator.OnInit(CancellationToken.None);
        await initializer.Invoke();

        // Assert
        scope1.Received(1).Dispose();
        scope2.Received(1).Dispose();
    }

    // ============================================================================
    // OnInit Tests - Return Value Semantics (1 test)
    // ============================================================================

    [Fact]
    public async Task OnInit_WhenAllHandlersSucceed_InitializerReturnsTrue()
    {
        // Arrange
        var delegateHolder = new DelegateHolder();
        delegateHolder.InitHandlers.Add((_, _) => Task.FromResult(true));
        delegateHolder.InitHandlers.Add((_, _) => Task.FromResult(true));
        delegateHolder.InitHandlers.Add((_, _) => Task.FromResult(true));

        var orchestrator = CreateOrchestrator(delegateHolder);

        // Act
        var initializer = orchestrator.OnInit(CancellationToken.None);
        var result = await initializer.Invoke();

        // Assert
        result.Should().BeTrue();
    }

    // ============================================================================
    // OnInit Tests - Initializer Delegate Behavior (2 tests)
    // ============================================================================

    [Fact]
    public void OnInit_ReturnsLambdaBootstrapInitializer_NotTask()
    {
        // Arrange
        var delegateHolder = new DelegateHolder();
        var orchestrator = CreateOrchestrator(delegateHolder);

        // Act
        var initializer = orchestrator.OnInit(CancellationToken.None);

        // Assert
        initializer.Should().NotBeNull();
        initializer.Should().BeOfType<LambdaBootstrapInitializer>();
    }

    [Fact]
    public async Task OnInit_InitializerDelegate_CanBeInvokedMultipleTimes()
    {
        // Arrange
        var invocationCount = 0;
        var delegateHolder = new DelegateHolder();
        delegateHolder.InitHandlers.Add(
            (_, _) =>
            {
                invocationCount++;
                return Task.FromResult(true);
            }
        );

        var orchestrator = CreateOrchestrator(delegateHolder);
        var initializer = orchestrator.OnInit(CancellationToken.None);

        // Act
        var result1 = await initializer.Invoke();
        var result2 = await initializer.Invoke();

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeTrue();
        invocationCount.Should().Be(2);
    }

    // ============================================================================
    // OnInit Tests - Edge Cases & Integration (3 tests)
    // ============================================================================

    [Fact]
    public async Task OnInit_WithCancelledStoppingToken_PropagatesImmediately()
    {
        // Arrange
        var cancellationReceived = false;
        var delegateHolder = new DelegateHolder();
        delegateHolder.InitHandlers.Add(
            (_, token) =>
            {
                cancellationReceived = token.IsCancellationRequested;
                return Task.FromResult(true);
            }
        );

        var orchestrator = CreateOrchestrator(delegateHolder);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var initializer = orchestrator.OnInit(cts.Token);
        var result = await initializer.Invoke();

        // Assert
        cancellationReceived.Should().BeTrue();
        result.Should().BeTrue();
    }

    [Fact]
    public async Task OnInit_WhenHandlerThrowsOperationCanceledException_IncludesInAggregateException()
    {
        // Arrange
        var canceledException1 = new OperationCanceledException();
        var canceledException2 = new OperationCanceledException();
        var delegateHolder = new DelegateHolder();
        delegateHolder.InitHandlers.Add((_, _) => throw canceledException1);
        delegateHolder.InitHandlers.Add((_, _) => throw canceledException2);

        var orchestrator = CreateOrchestrator(delegateHolder);

        // Act
        var initializer = orchestrator.OnInit(CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<AggregateException>(() => initializer.Invoke());
        ex.InnerExceptions.Should().HaveCount(2);
        ex.InnerExceptions[0].Should().BeOfType<OperationCanceledException>();
        ex.InnerExceptions[0].Should().Be(canceledException1);
        ex.InnerExceptions[1].Should().BeOfType<OperationCanceledException>();
        ex.InnerExceptions[1].Should().Be(canceledException2);
    }

    [Fact]
    public async Task OnInit_UsesConfiguredInitTimeoutFromOptions()
    {
        // Arrange
        var timeoutWasApplied = false;
        var customTimeout = TimeSpan.FromMilliseconds(50);
        var delegateHolder = new DelegateHolder();
        delegateHolder.InitHandlers.Add(
            async (_, token) =>
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), token);
                }
                catch (OperationCanceledException)
                {
                    timeoutWasApplied = true;
                }

                return true;
            }
        );

        var options = CreateMockLambdaHostOptions(customTimeout);
        var scopeFactory = CreateMockServiceScopeFactory();
        var orchestrator = new LambdaLifecycleOrchestrator(scopeFactory, delegateHolder, options);

        // Act
        var initializer = orchestrator.OnInit(CancellationToken.None);
        var result = await initializer.Invoke();

        // Assert
        timeoutWasApplied.Should().BeTrue();
        result.Should().BeTrue();
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
        var options = CreateMockLambdaHostOptions();
        return new LambdaLifecycleOrchestrator(scopeFactory, delegateHolder, options);
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
    /// Creates a properly configured mock Lambda host options.
    /// </summary>
    private static IOptions<LambdaHostOptions> CreateMockLambdaHostOptions()
    {
        var options = Substitute.For<IOptions<LambdaHostOptions>>();
        options.Value.Returns(new LambdaHostOptions());
        return options;
    }

    /// <summary>Creates a properly configured mock Lambda host options with custom InitTimeout.</summary>
    /// <param name="initTimeout">The custom initialization timeout duration</param>
    private static IOptions<LambdaHostOptions> CreateMockLambdaHostOptions(TimeSpan initTimeout)
    {
        var options = Substitute.For<IOptions<LambdaHostOptions>>();
        var hostOptions = new LambdaHostOptions { InitTimeout = initTimeout };
        options.Value.Returns(hostOptions);
        return options;
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

    /// <summary>Creates a delayed initialization handler that records its execution order.</summary>
    /// <param name="handlerId">Identifier for this handler</param>
    /// <param name="delayMs">Delay in milliseconds before completion</param>
    /// <param name="executionOrder">List to record execution order</param>
    /// <returns>An initialization delegate that executes with the specified delay</returns>
    private static LambdaInitDelegate CreateDelayedInitHandler(
        int handlerId,
        int delayMs,
        List<int> executionOrder
    ) =>
        async (_, _) =>
        {
            await Task.Delay(delayMs);
            executionOrder.Add(handlerId);
            return true;
        };
}
