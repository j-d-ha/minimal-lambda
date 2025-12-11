using AwsLambda.Host;
using Microsoft.Extensions.Options;

namespace MinimalLambda.UnitTests.Core.Runtime;

[TestSubject(typeof(LambdaHandlerComposer))]
public class LambdaHandlerComposerTests
{
    private readonly Fixture _fixture = new();

    #region Error Handling Tests

    [Fact]
    public async Task RequestHandler_PropagatesHandlerException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test exception");
        LambdaInvocationDelegate handler = async context =>
        {
            await Task.CompletedTask;
            throw expectedException;
        };
        _fixture.SetInvocationHandler(handler);

        var composer = _fixture.CreateComposer();
        var requestHandler = composer.CreateHandler(CancellationToken.None);

        // Act & Assert
        var act = () => requestHandler(new MemoryStream(), _fixture.LambdaContext);
        await act.Should().ThrowExactlyAsync<InvalidOperationException>();
    }

    #endregion

    /// <summary>Fixture for setting up mocks and dependencies for LambdaHandlerComposer tests.</summary>
    private class Fixture
    {
        public Fixture()
        {
            LambdaInvocationBuilderFactory = Substitute.For<ILambdaInvocationBuilderFactory>();
            CancellationFactory = Substitute.For<ILambdaCancellationFactory>();
            Options = Microsoft.Extensions.Options.Options.Create(new LambdaHostedServiceOptions());
            LambdaHostContextFactory = Substitute.For<ILambdaHostContextFactory>();
            InvocationDataFeatureFactory = Substitute.For<IInvocationDataFeatureFactory>();

            InvocationBuilder = Substitute.For<ILambdaInvocationBuilder>();
            CancellationTokenSource = new CancellationTokenSource();
            LambdaContext = Substitute.For<ILambdaContext>();
            ResponseFeature = Substitute.For<IResponseFeature>();
            LambdaHostContext = Substitute.For<ILambdaHostContext, IAsyncDisposable>();

            SetupDefaults();
        }

        public ILambdaCancellationFactory CancellationFactory { get; }
        public CancellationTokenSource CancellationTokenSource { get; }
        public ILambdaInvocationBuilder InvocationBuilder { get; }
        public IInvocationDataFeatureFactory InvocationDataFeatureFactory { get; }
        public ILambdaContext LambdaContext { get; }
        public ILambdaHostContext LambdaHostContext { get; }
        public ILambdaHostContextFactory LambdaHostContextFactory { get; }
        public ILambdaInvocationBuilderFactory LambdaInvocationBuilderFactory { get; }
        public IOptions<LambdaHostedServiceOptions> Options { get; }
        public IResponseFeature ResponseFeature { get; }

        /// <summary>Sets up default mock behaviors.</summary>
        private void SetupDefaults()
        {
            InvocationBuilder.Build().Returns(async context => { });
            InvocationBuilder.Properties.Returns(new Dictionary<string, object?>());
            LambdaInvocationBuilderFactory.CreateBuilder().Returns(InvocationBuilder);

            CancellationFactory
                .NewCancellationTokenSource(Arg.Any<ILambdaContext>())
                .Returns(CancellationTokenSource);

            // Create a mock features collection
            var mockFeatures = Substitute.For<IFeatureCollection>();
            mockFeatures.Get<IResponseFeature>().Returns(ResponseFeature);

            // Create a mock invocation data feature with response stream
            var mockInvocationDataFeature = Substitute.For<IInvocationDataFeature>();
            mockInvocationDataFeature.ResponseStream.Returns(new MemoryStream());
            InvocationDataFeatureFactory
                .Create(Arg.Any<Stream>())
                .Returns(mockInvocationDataFeature);

            // Set up the context factory to return a mock context for any Create call
            LambdaHostContextFactory
                .Create(
                    Arg.Any<ILambdaContext>(),
                    Arg.Any<IDictionary<string, object?>>(),
                    Arg.Any<CancellationToken>()
                )
                .Returns(info =>
                {
                    // Create a new mock context for each call
                    LambdaHostContext.Features.Returns(mockFeatures);
                    ((IAsyncDisposable)LambdaHostContext)
                        .DisposeAsync()
                        .Returns(ValueTask.CompletedTask);
                    return LambdaHostContext;
                });
        }

        /// <summary>Creates a LambdaHandlerComposer with the configured mocks.</summary>
        public LambdaHandlerComposer CreateComposer() =>
            new(
                LambdaInvocationBuilderFactory,
                CancellationFactory,
                Options,
                LambdaHostContextFactory,
                InvocationDataFeatureFactory
            );

        /// <summary>Sets the invocation handler that will be built by the builder.</summary>
        public void SetInvocationHandler(LambdaInvocationDelegate handler) =>
            InvocationBuilder.Build().Returns(handler);

        /// <summary>Creates a fresh cancellation token source for a test.</summary>
        public CancellationTokenSource CreateNewCancellationTokenSource()
        {
            var newSource = new CancellationTokenSource();
            CancellationFactory
                .NewCancellationTokenSource(Arg.Any<ILambdaContext>())
                .Returns(newSource);
            return newSource;
        }
    }

    #region Constructor Validation Tests

    [Theory]
    [InlineData(0)] // LambdaInvocationBuilderFactory
    [InlineData(1)] // CancellationFactory
    [InlineData(2)] // Options
    [InlineData(3)] // LambdaHostContextFactory
    [InlineData(4)] // InvocationDataFeatureFactory
    public void Constructor_WithNullParameter_ThrowsArgumentNullException(int parameterIndex)
    {
        // Arrange
        var invocationBuilderFactory =
            parameterIndex == 0 ? null : _fixture.LambdaInvocationBuilderFactory;
        var cancellationFactory = parameterIndex == 1 ? null : _fixture.CancellationFactory;
        var options = parameterIndex == 2 ? null : _fixture.Options;
        var contextFactory = parameterIndex == 3 ? null : _fixture.LambdaHostContextFactory;
        var invocationDataFeatureFactory =
            parameterIndex == 4 ? null : _fixture.InvocationDataFeatureFactory;

        // Act & Assert
        var act = () =>
            new LambdaHandlerComposer(
                invocationBuilderFactory!,
                cancellationFactory!,
                options!,
                contextFactory!,
                invocationDataFeatureFactory!
            );
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithValidParameters_SuccessfullyConstructs()
    {
        // Act
        var composer = _fixture.CreateComposer();

        // Assert
        composer.Should().NotBeNull();
        composer.Should().BeAssignableTo<ILambdaHandlerFactory>();
    }

    #endregion

    #region CreateHandler Method Tests

    [Fact]
    public void CreateHandler_ReturnsValidHandlerFunction()
    {
        // Arrange
        var composer = _fixture.CreateComposer();

        // Act
        var handler = composer.CreateHandler(CancellationToken.None);

        // Assert
        handler.Should().NotBeNull();
    }

    [Fact]
    public void CreateHandler_CreatesInvocationBuilder()
    {
        // Arrange
        var composer = _fixture.CreateComposer();

        // Act
        composer.CreateHandler(CancellationToken.None);

        // Assert
        _fixture.LambdaInvocationBuilderFactory.Received(1).CreateBuilder();
    }

    [Fact]
    public void CreateHandler_BuildsInvocationBuilder()
    {
        // Arrange
        var composer = _fixture.CreateComposer();

        // Act
        composer.CreateHandler(CancellationToken.None);

        // Assert
        _fixture.InvocationBuilder.Received(1).Build();
    }

    [Fact]
    public void CreateHandler_InvokesConfigureHandlerBuilder_WhenProvided()
    {
        // Arrange
        var configureHandlerBuilderInvoked = false;
        Action<ILambdaInvocationBuilder> configureAction = builder =>
        {
            configureHandlerBuilderInvoked = true;
        };

        var lambdaOptions = new LambdaHostedServiceOptions
        {
            ConfigureHandlerBuilder = configureAction,
        };
        var options = Microsoft.Extensions.Options.Options.Create(lambdaOptions);

        var composer = new LambdaHandlerComposer(
            _fixture.LambdaInvocationBuilderFactory,
            _fixture.CancellationFactory,
            options,
            _fixture.LambdaHostContextFactory,
            _fixture.InvocationDataFeatureFactory
        );

        // Act
        composer.CreateHandler(CancellationToken.None);

        // Assert
        configureHandlerBuilderInvoked.Should().BeTrue();
    }

    [Fact]
    public void CreateHandler_DoesNotInvokeConfigureHandlerBuilder_WhenNotProvided()
    {
        // Arrange
        var composer = _fixture.CreateComposer();

        // Act & Assert (should not throw)
        var handler = composer.CreateHandler(CancellationToken.None);
        handler.Should().NotBeNull();
    }

    #endregion

    #region Request Handler Behavior Tests

    [Fact]
    public async Task RequestHandler_CreatesCancellationTokenSource()
    {
        // Arrange
        var composer = _fixture.CreateComposer();
        var handler = composer.CreateHandler(CancellationToken.None);

        // Act
        await handler(new MemoryStream(), _fixture.LambdaContext);

        // Assert
        _fixture
            .CancellationFactory.Received(1)
            .NewCancellationTokenSource(_fixture.LambdaContext);
    }

    [Fact]
    public async Task RequestHandler_InvokesMiddlewarePipeline()
    {
        // Arrange
        var handlerInvoked = false;
        LambdaInvocationDelegate handler = async context =>
        {
            handlerInvoked = true;
            await Task.CompletedTask;
        };
        _fixture.SetInvocationHandler(handler);

        var composer = _fixture.CreateComposer();
        var requestHandler = composer.CreateHandler(CancellationToken.None);

        // Act
        await requestHandler(new MemoryStream(), _fixture.LambdaContext);

        // Assert
        handlerInvoked.Should().BeTrue();
    }

    [Fact]
    public async Task RequestHandler_ReturnsResponseStream()
    {
        // Arrange
        var composer = _fixture.CreateComposer();
        var handler = composer.CreateHandler(CancellationToken.None);

        // Act
        var responseStream = await handler(new MemoryStream(), _fixture.LambdaContext);

        // Assert
        responseStream.Should().NotBeNull();
        responseStream.Should().BeOfType<MemoryStream>();
    }

    [Fact]
    public async Task RequestHandler_DisposesResources_AfterInvocation()
    {
        // Arrange
        var cancellationTokenSource = _fixture.CreateNewCancellationTokenSource();
        var composer = _fixture.CreateComposer();
        var handler = composer.CreateHandler(CancellationToken.None);

        // Act
        await handler(new MemoryStream(), _fixture.LambdaContext);

        // Assert
        // After invocation, the cancellation token source should have been disposed
        var act = () => cancellationTokenSource.Token;
        act.Should().ThrowExactly<ObjectDisposedException>();
    }

    #endregion
}
