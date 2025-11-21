using Amazon.Lambda.Core;
using AwesomeAssertions;
using AwsLambda.Host.Core.Features;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace AwsLambda.Host.UnitTests.Core.Runtime;

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
            FeatureCollectionFactory = Substitute.For<IFeatureCollectionFactory>();
            LambdaInvocationBuilderFactory = Substitute.For<ILambdaInvocationBuilderFactory>();
            CancellationFactory = Substitute.For<ILambdaCancellationFactory>();
            ScopeFactory = Substitute.For<IServiceScopeFactory>();
            Options = Microsoft.Extensions.Options.Options.Create(new LambdaHostedServiceOptions());

            InvocationBuilder = Substitute.For<ILambdaInvocationBuilder>();
            FeatureCollection = Substitute.For<IFeatureCollection>();
            CancellationTokenSource = new CancellationTokenSource();
            LambdaContext = Substitute.For<ILambdaContext>();

            SetupDefaults();
        }

        public ILambdaCancellationFactory CancellationFactory { get; }
        public CancellationTokenSource CancellationTokenSource { get; }
        public IFeatureCollection FeatureCollection { get; }
        public IFeatureCollectionFactory FeatureCollectionFactory { get; }
        public ILambdaInvocationBuilder InvocationBuilder { get; }
        public ILambdaContext LambdaContext { get; }
        public ILambdaInvocationBuilderFactory LambdaInvocationBuilderFactory { get; }
        public IOptions<LambdaHostedServiceOptions> Options { get; }
        public IServiceScopeFactory ScopeFactory { get; }

        /// <summary>Sets up default mock behaviors.</summary>
        private void SetupDefaults()
        {
            InvocationBuilder.Build().Returns(async context => { });
            LambdaInvocationBuilderFactory.CreateBuilder().Returns(InvocationBuilder);

            CancellationFactory
                .NewCancellationTokenSource(Arg.Any<ILambdaContext>())
                .Returns(CancellationTokenSource);

            FeatureCollectionFactory.Create().Returns(FeatureCollection);
        }

        /// <summary>Creates a LambdaHandlerComposer with the configured mocks.</summary>
        public LambdaHandlerComposer CreateComposer() =>
            new(
                FeatureCollectionFactory,
                LambdaInvocationBuilderFactory,
                CancellationFactory,
                ScopeFactory,
                Options
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
    [InlineData(0)] // FeatureCollectionFactory
    [InlineData(1)] // LambdaInvocationBuilderFactory
    [InlineData(2)] // CancellationFactory
    [InlineData(3)] // ScopeFactory
    [InlineData(4)] // Options
    public void Constructor_WithNullParameter_ThrowsArgumentNullException(int parameterIndex)
    {
        // Arrange
        var featureCollectionFactory =
            parameterIndex == 0 ? null : _fixture.FeatureCollectionFactory;
        var invocationBuilderFactory =
            parameterIndex == 1 ? null : _fixture.LambdaInvocationBuilderFactory;
        var cancellationFactory = parameterIndex == 2 ? null : _fixture.CancellationFactory;
        var scopeFactory = parameterIndex == 3 ? null : _fixture.ScopeFactory;
        var options = parameterIndex == 4 ? null : _fixture.Options;

        // Act & Assert
        var act = () =>
            new LambdaHandlerComposer(
                featureCollectionFactory!,
                invocationBuilderFactory!,
                cancellationFactory!,
                scopeFactory!,
                options!
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
        Action<ILambdaInvocationBuilder>? configureAction = builder =>
        {
            configureHandlerBuilderInvoked = true;
        };

        var lambdaOptions = new LambdaHostedServiceOptions
        {
            ConfigureHandlerBuilder = configureAction,
        };
        var options = Microsoft.Extensions.Options.Options.Create(lambdaOptions);

        var composer = new LambdaHandlerComposer(
            _fixture.FeatureCollectionFactory,
            _fixture.LambdaInvocationBuilderFactory,
            _fixture.CancellationFactory,
            _fixture.ScopeFactory,
            options
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
    public async Task RequestHandler_CreatesFeatureCollection()
    {
        // Arrange
        var composer = _fixture.CreateComposer();
        var handler = composer.CreateHandler(CancellationToken.None);

        // Act
        await handler(new MemoryStream(), _fixture.LambdaContext);

        // Assert
        _fixture.FeatureCollectionFactory.Received(1).Create();
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

    #region Cancellation Token Handling Tests

    [Fact]
    public async Task RequestHandler_LinksCancellationTokensCorrectly()
    {
        // Arrange
        var capturedContext = default(ILambdaHostContext);
        LambdaInvocationDelegate handler = async context =>
        {
            capturedContext = context;
            await Task.CompletedTask;
        };
        _fixture.SetInvocationHandler(handler);

        var composer = _fixture.CreateComposer();
        var requestHandler = composer.CreateHandler(CancellationToken.None);

        // Act
        await requestHandler(new MemoryStream(), _fixture.LambdaContext);

        // Assert
        capturedContext.Should().NotBeNull();
        capturedContext!.CancellationToken.Should().NotBe(CancellationToken.None);
    }

    [Fact]
    public async Task RequestHandler_CancellationToken_IsCancelledWhenStoppingTokenCancelled()
    {
        // Arrange
        var capturedCancellationToken = CancellationToken.None;
        LambdaInvocationDelegate handler = async context =>
        {
            capturedCancellationToken = context.CancellationToken;
            await Task.CompletedTask;
        };
        _fixture.SetInvocationHandler(handler);

        var lambdaCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        _fixture
            .CancellationFactory.NewCancellationTokenSource(Arg.Any<ILambdaContext>())
            .Returns(lambdaCancellationTokenSource);

        using var stoppingTokenSource = new CancellationTokenSource();
        var composer = _fixture.CreateComposer();
        var requestHandler = composer.CreateHandler(stoppingTokenSource.Token);

        // Act
        stoppingTokenSource.Cancel();
        await requestHandler(new MemoryStream(), _fixture.LambdaContext);

        // Assert
        capturedCancellationToken.IsCancellationRequested.Should().BeTrue();
    }

    #endregion
}
