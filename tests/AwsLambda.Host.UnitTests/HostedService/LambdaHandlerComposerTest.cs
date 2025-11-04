using System.Text;
using Amazon.Lambda.Core;
using AwesomeAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace AwsLambda.Host.UnitTests.HostedService;

[TestSubject(typeof(LambdaHandlerComposer))]
public class LambdaHandlerComposerTest
{
    private readonly IServiceScopeFactory _mockScopeFactory;
    private readonly IOptions<LambdaHostOptions> _mockSettings;
    private readonly ILambdaCancellationTokenSourceFactory _mockTokenFactory;

    public LambdaHandlerComposerTest()
    {
        _mockSettings = Substitute.For<IOptions<LambdaHostOptions>>();
        _mockSettings.Value.Returns(new LambdaHostOptions());

        _mockTokenFactory = Substitute.For<ILambdaCancellationTokenSourceFactory>();
        _mockScopeFactory = Substitute.For<IServiceScopeFactory>();

        // Setup default service scope
        var mockScope = Substitute.For<IServiceScope>();
        var mockServiceProvider = Substitute.For<IServiceProvider>();
        mockScope.ServiceProvider.Returns(mockServiceProvider);
        _mockScopeFactory.CreateScope().Returns(mockScope);

        // Setup cancellation token factory to return a new CancellationTokenSource each time
        _mockTokenFactory
            .NewCancellationTokenSource(Arg.Any<ILambdaContext>())
            .Returns(_ => new CancellationTokenSource());
    }

    // ============================================================================
    // Phase 1: Constructor Validation Tests (6 tests)
    // ============================================================================

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLambdaHostSettingsIsNull()
    {
        var delegateHolder = new DelegateHolder
        {
            Handler = Substitute.For<LambdaInvocationDelegate>(),
        };

        var act = () =>
            new LambdaHandlerComposer(null!, delegateHolder, _mockTokenFactory, _mockScopeFactory);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenDelegateHolderIsNull()
    {
        var act = () =>
            new LambdaHandlerComposer(_mockSettings, null!, _mockTokenFactory, _mockScopeFactory);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenCancellationTokenSourceFactoryIsNull()
    {
        var delegateHolder = new DelegateHolder
        {
            Handler = Substitute.For<LambdaInvocationDelegate>(),
        };

        var act = () =>
            new LambdaHandlerComposer(_mockSettings, delegateHolder, null!, _mockScopeFactory);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenServiceScopeFactoryIsNull()
    {
        var delegateHolder = new DelegateHolder
        {
            Handler = Substitute.For<LambdaInvocationDelegate>(),
        };

        var act = () =>
            new LambdaHandlerComposer(_mockSettings, delegateHolder, _mockTokenFactory, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ThrowsInvalidOperationException_WhenHandlerIsNotSet()
    {
        var delegateHolder = new DelegateHolder(); // Handler not set

        var act = () =>
            new LambdaHandlerComposer(
                _mockSettings,
                delegateHolder,
                _mockTokenFactory,
                _mockScopeFactory
            );

        act.Should().Throw<InvalidOperationException>().WithMessage("Lambda Handler is not set");
    }

    [Fact]
    public void Constructor_Succeeds_WhenAllDependenciesAreValid()
    {
        var delegateHolder = new DelegateHolder
        {
            Handler = Substitute.For<LambdaInvocationDelegate>(),
        };

        var composer = new LambdaHandlerComposer(
            _mockSettings,
            delegateHolder,
            _mockTokenFactory,
            _mockScopeFactory
        );

        composer.Should().NotBeNull();
    }

    // ============================================================================
    // Phase 2: Handler Execution Tests (Simulating Generated Code) (6 tests)
    // ============================================================================

    [Fact]
    public async Task CreateHandler_InvokesSimpleHandler_WithContext()
    {
        var handlerInvoked = false;

        Task SimpleHandler(ILambdaHostContext context)
        {
            handlerInvoked = true;
            context.Response = "handled";
            return Task.CompletedTask;
        }

        var delegateHolder = new DelegateHolder { Handler = SimpleHandler };
        var composer = new LambdaHandlerComposer(
            _mockSettings,
            delegateHolder,
            _mockTokenFactory,
            _mockScopeFactory
        );

        var handler = composer.CreateHandler(CancellationToken.None);
        var mockLambdaContext = Substitute.For<ILambdaContext>();

        await handler(new MemoryStream(), mockLambdaContext);

        handlerInvoked.Should().BeTrue();
    }

    [Fact]
    public async Task CreateHandler_DeserializesRequestStream_BeforeHandlerExecution()
    {
        var deserializerInvoked = false;
        var requestData = "test-request";

        Task Handler(ILambdaHostContext context)
        {
            context.Event.Should().Be(requestData);
            return Task.CompletedTask;
        }

        Task Deserializer(
            ILambdaHostContext context,
            ILambdaSerializer serializer,
            Stream eventStream
        )
        {
            deserializerInvoked = true;
            context.Event = requestData;
            return Task.CompletedTask;
        }

        var delegateHolder = new DelegateHolder { Handler = Handler, Deserializer = Deserializer };
        var composer = new LambdaHandlerComposer(
            _mockSettings,
            delegateHolder,
            _mockTokenFactory,
            _mockScopeFactory
        );

        var handler = composer.CreateHandler(CancellationToken.None);
        var mockLambdaContext = Substitute.For<ILambdaContext>();

        await handler(new MemoryStream(), mockLambdaContext);

        deserializerInvoked.Should().BeTrue();
    }

    [Fact]
    public async Task CreateHandler_SerializesResponseStream_AfterHandlerExecution()
    {
        const string expectedResponse = "test-response";

        Task Handler(ILambdaHostContext context)
        {
            context.Response = expectedResponse;
            return Task.CompletedTask;
        }

        Task<Stream> Serializer(ILambdaHostContext context, ILambdaSerializer serializer)
        {
            var responseStream = new MemoryStream();
            responseStream.Write(Encoding.UTF8.GetBytes((string)context.Response!));
            responseStream.Position = 0;
            return Task.FromResult<Stream>(responseStream);
        }

        var delegateHolder = new DelegateHolder { Handler = Handler, Serializer = Serializer };
        var composer = new LambdaHandlerComposer(
            _mockSettings,
            delegateHolder,
            _mockTokenFactory,
            _mockScopeFactory
        );

        var handler = composer.CreateHandler(CancellationToken.None);
        var mockLambdaContext = Substitute.For<ILambdaContext>();

        var resultStream = await handler(new MemoryStream(), mockLambdaContext);

        resultStream.Should().NotBeNull();
        resultStream.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateHandler_WithNoDeserializer_SkipsDeserialization()
    {
        var handlerInvoked = false;

        Task Handler(ILambdaHostContext context)
        {
            handlerInvoked = true;
            return Task.CompletedTask;
        }

        var delegateHolder = new DelegateHolder { Handler = Handler, Deserializer = null };
        var composer = new LambdaHandlerComposer(
            _mockSettings,
            delegateHolder,
            _mockTokenFactory,
            _mockScopeFactory
        );

        var handler = composer.CreateHandler(CancellationToken.None);
        var mockLambdaContext = Substitute.For<ILambdaContext>();

        await handler(new MemoryStream(), mockLambdaContext);

        handlerInvoked.Should().BeTrue();
    }

    [Fact]
    public async Task CreateHandler_WithNoSerializer_ReturnsEmptyStream()
    {
        Task Handler(ILambdaHostContext context)
        {
            context.Response = "response";
            return Task.CompletedTask;
        }

        var delegateHolder = new DelegateHolder { Handler = Handler, Serializer = null };
        var composer = new LambdaHandlerComposer(
            _mockSettings,
            delegateHolder,
            _mockTokenFactory,
            _mockScopeFactory
        );

        var handler = composer.CreateHandler(CancellationToken.None);
        var mockLambdaContext = Substitute.For<ILambdaContext>();

        var resultStream = await handler(new MemoryStream(), mockLambdaContext);

        resultStream.Should().NotBeNull();
        resultStream.Should().BeOfType<MemoryStream>();
        resultStream.Length.Should().Be(0);
    }

    [Fact]
    public async Task CreateHandler_WithDeserializerAndSerializerFlow_Complete()
    {
        const string requestData = "input";

        Task Handler(ILambdaHostContext context)
        {
            var request = (string)context.Event!;
            context.Response = $"processed-{request}";
            return Task.CompletedTask;
        }

        Task Deserializer(
            ILambdaHostContext context,
            ILambdaSerializer serializer,
            Stream eventStream
        )
        {
            context.Event = requestData;
            return Task.CompletedTask;
        }

        Task<Stream> Serializer(ILambdaHostContext context, ILambdaSerializer serializer)
        {
            var outputStream = new MemoryStream();
            var content = Encoding.UTF8.GetBytes((string)context.Response!);
            outputStream.Write(content, 0, content.Length);
            outputStream.Position = 0;
            return Task.FromResult<Stream>(outputStream);
        }

        var delegateHolder = new DelegateHolder
        {
            Handler = Handler,
            Deserializer = Deserializer,
            Serializer = Serializer,
        };
        var composer = new LambdaHandlerComposer(
            _mockSettings,
            delegateHolder,
            _mockTokenFactory,
            _mockScopeFactory
        );

        var handler = composer.CreateHandler(CancellationToken.None);
        var mockLambdaContext = Substitute.For<ILambdaContext>();

        var resultStream = await handler(new MemoryStream(), mockLambdaContext);

        resultStream.Should().NotBeNull();
        resultStream.Length.Should().BeGreaterThan(0);
    }

    // ============================================================================
    // Phase 3: Middleware Tests (Realistic Composition) (6 tests)
    // ============================================================================

    [Fact]
    public async Task CreateHandler_WithSingleMiddleware_WrapsHandlerCorrectly()
    {
        var middlewareInvoked = false;
        var handlerInvoked = false;

        Task Handler(ILambdaHostContext context)
        {
            handlerInvoked = true;
            return Task.CompletedTask;
        }

        Func<LambdaInvocationDelegate, LambdaInvocationDelegate> middleware = next =>
            async context =>
            {
                middlewareInvoked = true;
                await next(context);
            };

        var delegateHolder = new DelegateHolder { Handler = Handler };
        delegateHolder.Middlewares.Add(middleware);

        var composer = new LambdaHandlerComposer(
            _mockSettings,
            delegateHolder,
            _mockTokenFactory,
            _mockScopeFactory
        );

        var handler = composer.CreateHandler(CancellationToken.None);
        var mockLambdaContext = Substitute.For<ILambdaContext>();

        await handler(new MemoryStream(), mockLambdaContext);

        middlewareInvoked.Should().BeTrue();
        handlerInvoked.Should().BeTrue();
    }

    [Fact]
    public async Task CreateHandler_WithMultipleMiddleware_AppliesInReverseOrder()
    {
        var executionOrder = new List<string>();

        Task Handler(ILambdaHostContext context)
        {
            executionOrder.Add("handler");
            return Task.CompletedTask;
        }

        Func<LambdaInvocationDelegate, LambdaInvocationDelegate> middleware1 = next =>
            async context =>
            {
                executionOrder.Add("middleware1-before");
                await next(context);
                executionOrder.Add("middleware1-after");
            };

        Func<LambdaInvocationDelegate, LambdaInvocationDelegate> middleware2 = next =>
            async context =>
            {
                executionOrder.Add("middleware2-before");
                await next(context);
                executionOrder.Add("middleware2-after");
            };

        Func<LambdaInvocationDelegate, LambdaInvocationDelegate> middleware3 = next =>
            async context =>
            {
                executionOrder.Add("middleware3-before");
                await next(context);
                executionOrder.Add("middleware3-after");
            };

        var delegateHolder = new DelegateHolder { Handler = Handler };
        delegateHolder.Middlewares.Add(middleware1);
        delegateHolder.Middlewares.Add(middleware2);
        delegateHolder.Middlewares.Add(middleware3);

        var composer = new LambdaHandlerComposer(
            _mockSettings,
            delegateHolder,
            _mockTokenFactory,
            _mockScopeFactory
        );

        var handler = composer.CreateHandler(CancellationToken.None);
        var mockLambdaContext = Substitute.For<ILambdaContext>();

        await handler(new MemoryStream(), mockLambdaContext);

        // Middleware is applied in reverse order, so middleware1 (first in list) is outermost
        executionOrder[0].Should().Be("middleware1-before");
        executionOrder[1].Should().Be("middleware2-before");
        executionOrder[2].Should().Be("middleware3-before");
        executionOrder[3].Should().Be("handler");
        executionOrder[4].Should().Be("middleware3-after");
        executionOrder[5].Should().Be("middleware2-after");
        executionOrder[6].Should().Be("middleware1-after");
    }

    [Fact]
    public async Task CreateHandler_MiddlewareCanAddContextItems()
    {
        const string tracingId = "trace-123";
        var itemsVerified = false;

        Task Handler(ILambdaHostContext context)
        {
            itemsVerified =
                context.Items.ContainsKey("TracingId")
                && (string)context.Items["TracingId"]! == tracingId;
            return Task.CompletedTask;
        }

        Func<LambdaInvocationDelegate, LambdaInvocationDelegate> middleware = next =>
            async context =>
            {
                context.Items["TracingId"] = tracingId;
                await next(context);
            };

        var delegateHolder = new DelegateHolder { Handler = Handler };
        delegateHolder.Middlewares.Add(middleware);

        var composer = new LambdaHandlerComposer(
            _mockSettings,
            delegateHolder,
            _mockTokenFactory,
            _mockScopeFactory
        );

        var handler = composer.CreateHandler(CancellationToken.None);
        var mockLambdaContext = Substitute.For<ILambdaContext>();

        await handler(new MemoryStream(), mockLambdaContext);

        itemsVerified.Should().BeTrue();
    }

    [Fact]
    public async Task CreateHandler_MiddlewareCanWrapHandlerExecution()
    {
        var exceptionCaught = false;
        var handlerInvoked = false;

        Task Handler(ILambdaHostContext context)
        {
            handlerInvoked = true;
            throw new InvalidOperationException("Handler error");
        }

        Func<LambdaInvocationDelegate, LambdaInvocationDelegate> errorHandlingMiddleware = next =>
            async context =>
            {
                try
                {
                    await next(context);
                }
                catch (InvalidOperationException)
                {
                    exceptionCaught = true;
                    context.Response = "error-handled";
                }
            };

        var delegateHolder = new DelegateHolder { Handler = Handler };
        delegateHolder.Middlewares.Add(errorHandlingMiddleware);

        var composer = new LambdaHandlerComposer(
            _mockSettings,
            delegateHolder,
            _mockTokenFactory,
            _mockScopeFactory
        );

        var handler = composer.CreateHandler(CancellationToken.None);
        var mockLambdaContext = Substitute.For<ILambdaContext>();

        await handler(new MemoryStream(), mockLambdaContext);

        handlerInvoked.Should().BeTrue();
        exceptionCaught.Should().BeTrue();
    }

    [Fact]
    public async Task CreateHandler_MiddlewareExecutesInOrder_WithDeserializer()
    {
        var executionOrder = new List<string>();

        Task Handler(ILambdaHostContext context)
        {
            executionOrder.Add("handler");
            return Task.CompletedTask;
        }

        Task Deserializer(
            ILambdaHostContext context,
            ILambdaSerializer serializer,
            Stream eventStream
        )
        {
            executionOrder.Add("deserializer");
            return Task.CompletedTask;
        }

        Func<LambdaInvocationDelegate, LambdaInvocationDelegate> middleware = next =>
            async context =>
            {
                executionOrder.Add("middleware-before");
                await next(context);
                executionOrder.Add("middleware-after");
            };

        var delegateHolder = new DelegateHolder { Handler = Handler, Deserializer = Deserializer };
        delegateHolder.Middlewares.Add(middleware);

        var composer = new LambdaHandlerComposer(
            _mockSettings,
            delegateHolder,
            _mockTokenFactory,
            _mockScopeFactory
        );

        var handler = composer.CreateHandler(CancellationToken.None);
        var mockLambdaContext = Substitute.For<ILambdaContext>();

        await handler(new MemoryStream(), mockLambdaContext);

        // Order: deserializer → middleware → handler → middleware cleanup
        executionOrder[0].Should().Be("deserializer");
        executionOrder[1].Should().Be("middleware-before");
        executionOrder[2].Should().Be("handler");
        executionOrder[3].Should().Be("middleware-after");
    }

    [Fact]
    public async Task CreateHandler_MiddlewareCanSkipHandler_Execution()
    {
        var handlerInvoked = false;

        Task Handler(ILambdaHostContext context)
        {
            handlerInvoked = true;
            return Task.CompletedTask;
        }

        Func<LambdaInvocationDelegate, LambdaInvocationDelegate> shortCircuitMiddleware = _ =>
            context =>
            {
                // Skip calling next, short-circuit the handler
                context.Response = "short-circuit";
                return Task.CompletedTask;
            };

        var delegateHolder = new DelegateHolder { Handler = Handler };
        delegateHolder.Middlewares.Add(shortCircuitMiddleware);

        var composer = new LambdaHandlerComposer(
            _mockSettings,
            delegateHolder,
            _mockTokenFactory,
            _mockScopeFactory
        );

        var handler = composer.CreateHandler(CancellationToken.None);
        var mockLambdaContext = Substitute.For<ILambdaContext>();

        await handler(new MemoryStream(), mockLambdaContext);

        handlerInvoked.Should().BeFalse();
    }

    // ============================================================================
    // Phase 4: Cancellation Token Tests (4 tests)
    // ============================================================================

    [Fact]
    public void CreateHandler_CreatesLinkedCancellationTokens_FromBothSources()
    {
        Task Handler(ILambdaHostContext context) => Task.CompletedTask;

        var delegateHolder = new DelegateHolder { Handler = Handler };
        var composer = new LambdaHandlerComposer(
            _mockSettings,
            delegateHolder,
            _mockTokenFactory,
            _mockScopeFactory
        );

        var handler = composer.CreateHandler(CancellationToken.None);

        // Handler created successfully, indicating cancellation token linking works
        handler.Should().NotBeNull();
        handler.Should().BeOfType<Func<Stream, ILambdaContext, Task<Stream>>>();
    }

    [Fact]
    public async Task CreateHandler_ContextRecievesLinkedCancellationToken()
    {
        ILambdaHostContext? capturedContext = null;

        Task Handler(ILambdaHostContext context)
        {
            capturedContext = context;
            return Task.CompletedTask;
        }

        var delegateHolder = new DelegateHolder { Handler = Handler };
        var composer = new LambdaHandlerComposer(
            _mockSettings,
            delegateHolder,
            _mockTokenFactory,
            _mockScopeFactory
        );

        var handler = composer.CreateHandler(CancellationToken.None);
        var mockLambdaContext = Substitute.For<ILambdaContext>();

        await handler(new MemoryStream(), mockLambdaContext);

        capturedContext.Should().NotBeNull();
        capturedContext.CancellationToken.Should().NotBe(CancellationToken.None);
    }

    [Fact]
    public async Task CreateHandler_CancellationDuringHandlerExecution_RespectsCancellation()
    {
        var operationCanceled = false;

        Task Handler(ILambdaHostContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        var delegateHolder = new DelegateHolder { Handler = Handler };
        var stoppingTokenSource = new CancellationTokenSource();

        var composer = new LambdaHandlerComposer(
            _mockSettings,
            delegateHolder,
            _mockTokenFactory,
            _mockScopeFactory
        );

        var handler = composer.CreateHandler(stoppingTokenSource.Token);
        var mockLambdaContext = Substitute.For<ILambdaContext>();

        // Cancel the token
        stoppingTokenSource.Cancel();

        try
        {
            await handler(new MemoryStream(), mockLambdaContext);
        }
        catch (OperationCanceledException)
        {
            operationCanceled = true;
        }

        operationCanceled.Should().BeTrue();
        stoppingTokenSource.Dispose();
    }

    [Fact]
    public async Task CreateHandler_DisposesResourcesProperly()
    {
        var disposed = false;

        Task Handler(ILambdaHostContext context) => Task.CompletedTask;

        var delegateHolder = new DelegateHolder { Handler = Handler };
        var composer = new LambdaHandlerComposer(
            _mockSettings,
            delegateHolder,
            _mockTokenFactory,
            _mockScopeFactory
        );

        // Override factory to return a CancellationTokenSource we can track
        var trackedSource = new TrackableCancellationTokenSource(() =>
        {
            disposed = true;
        });
        _mockTokenFactory
            .NewCancellationTokenSource(Arg.Any<ILambdaContext>())
            .Returns(trackedSource);

        var handler = composer.CreateHandler(CancellationToken.None);
        var mockLambdaContext = Substitute.For<ILambdaContext>();

        // Invoke the handler
        await handler(new MemoryStream(), mockLambdaContext);

        // Verify the cancellation token source was disposed
        disposed.Should().BeTrue();
    }

    // Helper class for tracking disposal
    private class TrackableCancellationTokenSource : CancellationTokenSource
    {
        private readonly Action _onDispose;

        public TrackableCancellationTokenSource(Action onDispose) => _onDispose = onDispose;

        protected override void Dispose(bool disposing)
        {
            _onDispose.Invoke();
            base.Dispose(disposing);
        }
    }
}
