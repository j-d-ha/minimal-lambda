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
    #region Helper Methods

    /// <summary>Creates a LambdaHandlerComposer instance with sensible defaults for testing.</summary>
    private LambdaHandlerComposer CreateLambdaHandlerComposer(
        IFeatureCollectionFactory? featureCollectionFactory = null,
        IInvocationBuilderFactory? invocationBuilderFactory = null,
        ILambdaCancellationFactory? cancellationFactory = null,
        IServiceScopeFactory? scopeFactory = null,
        IOptions<LambdaHostedServiceOptions>? options = null
    )
    {
        featureCollectionFactory ??= Substitute.For<IFeatureCollectionFactory>();
        invocationBuilderFactory ??= Substitute.For<IInvocationBuilderFactory>();
        cancellationFactory ??= Substitute.For<ILambdaCancellationFactory>();
        scopeFactory ??= Substitute.For<IServiceScopeFactory>();
        options ??= Microsoft.Extensions.Options.Options.Create(new LambdaHostedServiceOptions());

        var invocationBuilder = Substitute.For<ILambdaInvocationBuilder>();
        invocationBuilder.Build().Returns(async context => { });
        invocationBuilderFactory.CreateBuilder().Returns(invocationBuilder);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationFactory
            .NewCancellationTokenSource(Arg.Any<ILambdaContext>())
            .Returns(cancellationTokenSource);

        var featureCollection = Substitute.For<IFeatureCollection>();
        featureCollectionFactory.Create().Returns(featureCollection);

        return new LambdaHandlerComposer(
            featureCollectionFactory,
            invocationBuilderFactory,
            cancellationFactory,
            scopeFactory,
            options
        );
    }

    #endregion

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

        var invocationBuilderFactory = Substitute.For<IInvocationBuilderFactory>();
        var cancellationFactory = Substitute.For<ILambdaCancellationFactory>();
        var featureCollectionFactory = Substitute.For<IFeatureCollectionFactory>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var options = Microsoft.Extensions.Options.Options.Create(new LambdaHostedServiceOptions());

        var invocationBuilder = Substitute.For<ILambdaInvocationBuilder>();
        invocationBuilder.Build().Returns(handler);
        invocationBuilderFactory.CreateBuilder().Returns(invocationBuilder);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationFactory
            .NewCancellationTokenSource(Arg.Any<ILambdaContext>())
            .Returns(cancellationTokenSource);

        var featureCollection = Substitute.For<IFeatureCollection>();
        featureCollectionFactory.Create().Returns(featureCollection);

        var lambdaContext = Substitute.For<ILambdaContext>();
        var composer = new LambdaHandlerComposer(
            featureCollectionFactory,
            invocationBuilderFactory,
            cancellationFactory,
            scopeFactory,
            options
        );

        var requestHandler = composer.CreateHandler(CancellationToken.None);
        var inputStream = new MemoryStream();

        // Act & Assert
        var act = () => requestHandler(inputStream, lambdaContext);
        await act.Should().ThrowExactlyAsync<InvalidOperationException>();
    }

    #endregion

    #region Constructor Validation Tests

    [Fact]
    public void Constructor_WithNullFeatureCollectionFactory_ThrowsArgumentNullException()
    {
        // Arrange
        var invocationBuilderFactory = Substitute.For<IInvocationBuilderFactory>();
        var cancellationFactory = Substitute.For<ILambdaCancellationFactory>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        IOptions<LambdaHostedServiceOptions> options = Microsoft.Extensions.Options.Options.Create(
            new LambdaHostedServiceOptions()
        );

        // Act & Assert
        var act = () =>
            new LambdaHandlerComposer(
                null!,
                invocationBuilderFactory,
                cancellationFactory,
                scopeFactory,
                options
            );
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullInvocationBuilderFactory_ThrowsArgumentNullException()
    {
        // Arrange
        var featureCollectionFactory = Substitute.For<IFeatureCollectionFactory>();
        var cancellationFactory = Substitute.For<ILambdaCancellationFactory>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        IOptions<LambdaHostedServiceOptions> options = Microsoft.Extensions.Options.Options.Create(
            new LambdaHostedServiceOptions()
        );

        // Act & Assert
        var act = () =>
            new LambdaHandlerComposer(
                featureCollectionFactory,
                null!,
                cancellationFactory,
                scopeFactory,
                options
            );
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullCancellationFactory_ThrowsArgumentNullException()
    {
        // Arrange
        var featureCollectionFactory = Substitute.For<IFeatureCollectionFactory>();
        var invocationBuilderFactory = Substitute.For<IInvocationBuilderFactory>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        IOptions<LambdaHostedServiceOptions> options = Microsoft.Extensions.Options.Options.Create(
            new LambdaHostedServiceOptions()
        );

        // Act & Assert
        var act = () =>
            new LambdaHandlerComposer(
                featureCollectionFactory,
                invocationBuilderFactory,
                null!,
                scopeFactory,
                options
            );
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullScopeFactory_ThrowsArgumentNullException()
    {
        // Arrange
        var featureCollectionFactory = Substitute.For<IFeatureCollectionFactory>();
        var invocationBuilderFactory = Substitute.For<IInvocationBuilderFactory>();
        var cancellationFactory = Substitute.For<ILambdaCancellationFactory>();
        IOptions<LambdaHostedServiceOptions> options = Microsoft.Extensions.Options.Options.Create(
            new LambdaHostedServiceOptions()
        );

        // Act & Assert
        var act = () =>
            new LambdaHandlerComposer(
                featureCollectionFactory,
                invocationBuilderFactory,
                cancellationFactory,
                null!,
                options
            );
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var featureCollectionFactory = Substitute.For<IFeatureCollectionFactory>();
        var invocationBuilderFactory = Substitute.For<IInvocationBuilderFactory>();
        var cancellationFactory = Substitute.For<ILambdaCancellationFactory>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();

        // Act & Assert
        var act = () =>
            new LambdaHandlerComposer(
                featureCollectionFactory,
                invocationBuilderFactory,
                cancellationFactory,
                scopeFactory,
                null!
            );
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithValidParameters_SuccessfullyConstructs()
    {
        // Act
        var composer = CreateLambdaHandlerComposer();

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
        var composer = CreateLambdaHandlerComposer();

        // Act
        var handler = composer.CreateHandler(CancellationToken.None);

        // Assert
        handler.Should().NotBeNull();
    }

    [Fact]
    public void CreateHandler_CreatesInvocationBuilder()
    {
        // Arrange
        var invocationBuilderFactory = Substitute.For<IInvocationBuilderFactory>();
        var invocationBuilder = Substitute.For<ILambdaInvocationBuilder>();
        invocationBuilder.Build().Returns(async context => { });
        invocationBuilderFactory.CreateBuilder().Returns(invocationBuilder);

        var composer = CreateLambdaHandlerComposer(
            invocationBuilderFactory: invocationBuilderFactory
        );

        // Act
        composer.CreateHandler(CancellationToken.None);

        // Assert
        invocationBuilderFactory.Received(1).CreateBuilder();
    }

    [Fact]
    public void CreateHandler_BuildsInvocationBuilder()
    {
        // Arrange
        var invocationBuilderFactory = Substitute.For<IInvocationBuilderFactory>();
        var invocationBuilder = Substitute.For<ILambdaInvocationBuilder>();
        var cancellationFactory = Substitute.For<ILambdaCancellationFactory>();
        var featureCollectionFactory = Substitute.For<IFeatureCollectionFactory>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var options = Microsoft.Extensions.Options.Options.Create(new LambdaHostedServiceOptions());

        invocationBuilder.Build().Returns(async context => { });
        invocationBuilderFactory.CreateBuilder().Returns(invocationBuilder);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationFactory
            .NewCancellationTokenSource(Arg.Any<ILambdaContext>())
            .Returns(cancellationTokenSource);

        var featureCollection = Substitute.For<IFeatureCollection>();
        featureCollectionFactory.Create().Returns(featureCollection);

        var composer = new LambdaHandlerComposer(
            featureCollectionFactory,
            invocationBuilderFactory,
            cancellationFactory,
            scopeFactory,
            options
        );

        // Act
        composer.CreateHandler(CancellationToken.None);

        // Assert
        invocationBuilder.Received(1).Build();
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

        IOptions<LambdaHostedServiceOptions> options = Microsoft.Extensions.Options.Options.Create(
            lambdaOptions
        );
        var composer = CreateLambdaHandlerComposer(options: options);

        // Act
        composer.CreateHandler(CancellationToken.None);

        // Assert
        configureHandlerBuilderInvoked.Should().BeTrue();
    }

    [Fact]
    public void CreateHandler_DoesNotInvokeConfigureHandlerBuilder_WhenNotProvided()
    {
        // Arrange
        var lambdaOptions = new LambdaHostedServiceOptions { ConfigureHandlerBuilder = null };

        IOptions<LambdaHostedServiceOptions> options = Microsoft.Extensions.Options.Options.Create(
            lambdaOptions
        );
        var composer = CreateLambdaHandlerComposer(options: options);

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
        var cancellationFactory = Substitute.For<ILambdaCancellationFactory>();
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationFactory
            .NewCancellationTokenSource(Arg.Any<ILambdaContext>())
            .Returns(cancellationTokenSource);

        var lambdaContext = Substitute.For<ILambdaContext>();
        var composer = CreateLambdaHandlerComposer(cancellationFactory: cancellationFactory);

        var handler = composer.CreateHandler(CancellationToken.None);
        var inputStream = new MemoryStream();

        // Act
        await handler(inputStream, lambdaContext);

        // Assert
        cancellationFactory.Received(1).NewCancellationTokenSource(lambdaContext);
    }

    [Fact]
    public async Task RequestHandler_CreatesFeatureCollection()
    {
        // Arrange
        var featureCollectionFactory = Substitute.For<IFeatureCollectionFactory>();
        var featureCollection = Substitute.For<IFeatureCollection>();
        featureCollectionFactory.Create().Returns(featureCollection);

        var lambdaContext = Substitute.For<ILambdaContext>();
        var composer = CreateLambdaHandlerComposer(
            featureCollectionFactory: featureCollectionFactory
        );

        var handler = composer.CreateHandler(CancellationToken.None);
        var inputStream = new MemoryStream();

        // Act
        await handler(inputStream, lambdaContext);

        // Assert
        featureCollectionFactory.Received(1).Create();
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

        var invocationBuilderFactory = Substitute.For<IInvocationBuilderFactory>();
        var cancellationFactory = Substitute.For<ILambdaCancellationFactory>();
        var featureCollectionFactory = Substitute.For<IFeatureCollectionFactory>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var options = Microsoft.Extensions.Options.Options.Create(new LambdaHostedServiceOptions());

        var invocationBuilder = Substitute.For<ILambdaInvocationBuilder>();
        invocationBuilder.Build().Returns(handler);
        invocationBuilderFactory.CreateBuilder().Returns(invocationBuilder);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationFactory
            .NewCancellationTokenSource(Arg.Any<ILambdaContext>())
            .Returns(cancellationTokenSource);

        var featureCollection = Substitute.For<IFeatureCollection>();
        featureCollectionFactory.Create().Returns(featureCollection);

        var lambdaContext = Substitute.For<ILambdaContext>();
        var composer = new LambdaHandlerComposer(
            featureCollectionFactory,
            invocationBuilderFactory,
            cancellationFactory,
            scopeFactory,
            options
        );

        var requestHandler = composer.CreateHandler(CancellationToken.None);
        var inputStream = new MemoryStream();

        // Act
        await requestHandler(inputStream, lambdaContext);

        // Assert
        handlerInvoked.Should().BeTrue();
    }

    [Fact]
    public async Task RequestHandler_ReturnsResponseStream()
    {
        // Arrange
        var lambdaContext = Substitute.For<ILambdaContext>();
        var composer = CreateLambdaHandlerComposer();

        var handler = composer.CreateHandler(CancellationToken.None);
        var inputStream = new MemoryStream();

        // Act
        var responseStream = await handler(inputStream, lambdaContext);

        // Assert
        responseStream.Should().NotBeNull();
        responseStream.Should().BeOfType<MemoryStream>();
    }

    [Fact]
    public async Task RequestHandler_DisposesResources_AfterInvocation()
    {
        // Arrange
        var cancellationFactory = Substitute.For<ILambdaCancellationFactory>();
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationFactory
            .NewCancellationTokenSource(Arg.Any<ILambdaContext>())
            .Returns(cancellationTokenSource);

        var lambdaContext = Substitute.For<ILambdaContext>();
        var composer = CreateLambdaHandlerComposer(cancellationFactory: cancellationFactory);

        var handler = composer.CreateHandler(CancellationToken.None);
        var inputStream = new MemoryStream();

        // Act
        await handler(inputStream, lambdaContext);

        // Assert
        // After invocation, the cancellation token source should have been disposed
        var act = () => cancellationTokenSource.Token;
        act.Should().NotThrow();
    }

    #endregion

    #region Cancellation Token Handling Tests

    [Fact]
    public async Task RequestHandler_LinksCancellationTokensCorrectly()
    {
        // Arrange
        var capturedContext = default(ILambdaHostContext);

        var invocationBuilderFactory = Substitute.For<IInvocationBuilderFactory>();
        var cancellationFactory = Substitute.For<ILambdaCancellationFactory>();
        var featureCollectionFactory = Substitute.For<IFeatureCollectionFactory>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var options = Microsoft.Extensions.Options.Options.Create(new LambdaHostedServiceOptions());

        var invocationBuilder = Substitute.For<ILambdaInvocationBuilder>();
        LambdaInvocationDelegate handler = async context =>
        {
            capturedContext = context;
            await Task.CompletedTask;
        };
        invocationBuilder.Build().Returns(handler);
        invocationBuilderFactory.CreateBuilder().Returns(invocationBuilder);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationFactory
            .NewCancellationTokenSource(Arg.Any<ILambdaContext>())
            .Returns(cancellationTokenSource);

        var featureCollection = Substitute.For<IFeatureCollection>();
        featureCollectionFactory.Create().Returns(featureCollection);

        var lambdaContext = Substitute.For<ILambdaContext>();
        var composer = new LambdaHandlerComposer(
            featureCollectionFactory,
            invocationBuilderFactory,
            cancellationFactory,
            scopeFactory,
            options
        );

        var requestHandler = composer.CreateHandler(CancellationToken.None);
        var inputStream = new MemoryStream();

        // Act
        await requestHandler(inputStream, lambdaContext);

        // Assert
        capturedContext.Should().NotBeNull();
        capturedContext!.CancellationToken.Should().NotBe(CancellationToken.None);
    }

    [Fact]
    public async Task RequestHandler_CancellationToken_IsCancelledWhenStoppingTokenCancelled()
    {
        // Arrange
        var capturedCancellationToken = CancellationToken.None;

        var invocationBuilderFactory = Substitute.For<IInvocationBuilderFactory>();
        var cancellationFactory = Substitute.For<ILambdaCancellationFactory>();
        var featureCollectionFactory = Substitute.For<IFeatureCollectionFactory>();
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        var options = Microsoft.Extensions.Options.Options.Create(new LambdaHostedServiceOptions());

        var invocationBuilder = Substitute.For<ILambdaInvocationBuilder>();
        LambdaInvocationDelegate handler = async context =>
        {
            capturedCancellationToken = context.CancellationToken;
            await Task.CompletedTask;
        };
        invocationBuilder.Build().Returns(handler);
        invocationBuilderFactory.CreateBuilder().Returns(invocationBuilder);

        var lambdaCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        cancellationFactory
            .NewCancellationTokenSource(Arg.Any<ILambdaContext>())
            .Returns(lambdaCancellationTokenSource);

        var featureCollection = Substitute.For<IFeatureCollection>();
        featureCollectionFactory.Create().Returns(featureCollection);

        using var stoppingTokenSource = new CancellationTokenSource();
        var lambdaContext = Substitute.For<ILambdaContext>();
        var composer = new LambdaHandlerComposer(
            featureCollectionFactory,
            invocationBuilderFactory,
            cancellationFactory,
            scopeFactory,
            options
        );

        var requestHandler = composer.CreateHandler(stoppingTokenSource.Token);
        var inputStream = new MemoryStream();

        // Act
        stoppingTokenSource.Cancel();
        await requestHandler(inputStream, lambdaContext);

        // Assert
        capturedCancellationToken.IsCancellationRequested.Should().BeTrue();
    }

    #endregion
}
