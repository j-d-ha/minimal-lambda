using Microsoft.Extensions.DependencyInjection;
using MinimalLambda.Options;

namespace MinimalLambda.Testing.UnitTests;

public class SimpleLambdaTests : IClassFixture<LambdaApplicationFactory<SimpleLambda>>
{
    private readonly LambdaTestServer _server;

    public SimpleLambdaTests(LambdaApplicationFactory<SimpleLambda> factory)
    {
        factory.WithCancelationToken(TestContext.Current.CancellationToken);
        _server = factory.TestServer;
    }

    [Fact]
    public async Task SimpleLambda_ReturnsExpectedValue()
    {
        await using var factory = new LambdaApplicationFactory<SimpleLambda>().WithCancelationToken(
            TestContext.Current.CancellationToken
        );
        var setup = await factory.TestServer.StartAsync(TestContext.Current.CancellationToken);
        setup.InitStatus.Should().Be(InitStatus.InitCompleted);

        var response = await factory.TestServer.InvokeAsync<string, string>(
            "World",
            cancellationToken: TestContext.Current.CancellationToken
        );

        response.WasSuccess.Should().BeTrue();
        response.Should().NotBeNull();
        response.Response.Should().Be("Hello World!");
    }

    [Fact]
    public async Task SimpleLambda_WorksWhenStartIsNotCalled()
    {
        var response = await _server.InvokeAsync<string, string>(
            "World",
            TestContext.Current.CancellationToken
        );

        response.WasSuccess.Should().BeTrue();
        response.Should().NotBeNull();
        response.Response.Should().Be("Hello World!");
    }

    [Fact]
    public async Task SimpleLambda_WorksWhenInvokeCalledMultipleTimes()
    {
        // Launch 5 concurrent invocations
        var tasks = Enumerable
            .Range(1, 5)
            .Select(i =>
                _server.InvokeAsync<string, string>(
                    $"User{i}",
                    TestContext.Current.CancellationToken
                )
            )
            .ToArray();

        var responses = await Task.WhenAll(tasks);

        responses.Should().AllSatisfy(r => r.WasSuccess.Should().BeTrue());

        responses
            .Select(r => r.Response)
            .Should()
            .ContainInOrder(
                "Hello User1!",
                "Hello User2!",
                "Hello User3!",
                "Hello User4!",
                "Hello User5!"
            );
    }

    [Fact]
    public async Task SimpleLambda_ReturnsError()
    {
        var response = await _server.InvokeAsync<string, string>(
            "",
            TestContext.Current.CancellationToken
        );

        response.Should().NotBeNull();
        response.WasSuccess.Should().BeFalse();
        response.Error.Should().NotBeNull();
        response.Error.ErrorMessage.Should().Be("Name is required");
    }

    [Fact]
    public async Task SimpleLambda_ErrorsArePropagated()
    {
        await using var factory = new LambdaApplicationFactory<SimpleLambda>()
            .WithCancelationToken(TestContext.Current.CancellationToken)
            .WithHostBuilder(builder =>
            {
                builder.ConfigureServices(
                    (_, services) =>
                    {
                        services.Configure<LambdaHostOptions>(options =>
                        {
                            options.BootstrapOptions.RuntimeApiEndpoint = "http://localhost:3002";
                        });
                    }
                );
            });

        var act = async () =>
            await factory.TestServer.StartAsync(TestContext.Current.CancellationToken);

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage(
                "Unexpected request received from the Lambda HTTP handler: GET http://http//localhost:3002/2018-06-01/runtime/invocation/next"
            );
    }

    [Fact]
    public async Task SimpleLambda_WithPreCanceledToken_CancelsInvocation()
    {
        await using var factory = new LambdaApplicationFactory<SimpleLambda>().WithCancelationToken(
            TestContext.Current.CancellationToken
        );
        await factory.TestServer.StartAsync(TestContext.Current.CancellationToken);

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            factory.TestServer.InvokeAsync<string, string>("Jonas", cts.Token)
        );
    }
}
