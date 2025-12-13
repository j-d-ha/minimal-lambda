using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MinimalLambda.UnitTests;
using NSubstitute.ExceptionExtensions;

namespace MinimalLambda.Testing.UnitTests;

public class DiLambdaTests : IClassFixture<LambdaApplicationFactory<DiLambda>>
{
    private readonly LambdaTestServer _server;

    public DiLambdaTests(LambdaApplicationFactory<DiLambda> factory)
    {
        factory.WithCancelationToken(TestContext.Current.CancellationToken);
        _server = factory.TestServer;
    }

    [Fact]
    public async Task DiLambda_ReturnsExpectedValue()
    {
        var response = await _server.InvokeAsync<DiLambdaRequest, DiLambdaResponse>(
            new DiLambdaRequest("World"),
            TestContext.Current.CancellationToken
        );

        response.Should().NotBeNull();
        response.WasSuccess.Should().BeTrue();
        response.Response.Should().NotBeNull();
        response.Response.Message.Should().Be("Hello World!");
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task DiLambda_InitStopped(ILifecycleService lifecycleService)
    {
        await using var factory = new LambdaApplicationFactory<DiLambda>()
            .WithCancelationToken(TestContext.Current.CancellationToken)
            .WithHostBuilder(builder =>
                builder.ConfigureServices(
                    (_, services) =>
                    {
                        services.RemoveAll<ILifecycleService>();
                        services.AddSingleton<ILifecycleService>(_ => lifecycleService);
                    }
                )
            );

        lifecycleService.OnStart().Returns(false);

        var initResult = await factory.TestServer.StartAsync(TestContext.Current.CancellationToken);
        initResult.InitStatus.Should().Be(InitStatus.HostExited);
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task DiLambda_InitThrowsException(ILifecycleService lifecycleService)
    {
        await using var factory = new LambdaApplicationFactory<DiLambda>()
            .WithCancelationToken(TestContext.Current.CancellationToken)
            .WithHostBuilder(builder =>
                builder.ConfigureServices(
                    (_, services) =>
                    {
                        services.RemoveAll<ILifecycleService>();
                        services.AddSingleton<ILifecycleService>(_ => lifecycleService);
                    }
                )
            );

        lifecycleService.OnStart().Throws(new Exception("Test init error"));

        var initResult = await factory.TestServer.StartAsync(TestContext.Current.CancellationToken);
        initResult.InitStatus.Should().Be(InitStatus.InitError);
        initResult.Error.Should().NotBeNull();
        initResult
            .Error.ErrorMessage.Should()
            .Be("Encountered errors while running OnInit handlers: (Test init error)");
    }

    [Theory]
    [AutoNSubstituteData]
    internal async Task DiLambda_ShutdownThrowsException(ILifecycleService lifecycleService)
    {
        await using var factory = new LambdaApplicationFactory<DiLambda>()
            .WithCancelationToken(TestContext.Current.CancellationToken)
            .WithHostBuilder(builder =>
                builder.ConfigureServices(
                    (_, services) =>
                    {
                        services.RemoveAll<ILifecycleService>();
                        services.AddSingleton<ILifecycleService>(_ => lifecycleService);
                    }
                )
            );

        lifecycleService.OnStart().Returns(true);
        lifecycleService.When(x => x.OnStop()).Do(_ => throw new Exception("Test init error"));

        var initResult = await factory.TestServer.StartAsync(TestContext.Current.CancellationToken);
        initResult.InitStatus.Should().Be(InitStatus.InitCompleted);

        var act = async () =>
            await factory.TestServer.StopAsync(TestContext.Current.CancellationToken);

        (await act.Should().ThrowAsync<AggregateException>())
            .WithInnerException<AggregateException>()
            .WithInnerException<AggregateException>()
            .WithInnerException<Exception>()
            .WithMessage("Test init error");
    }
}
