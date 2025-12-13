using Microsoft.Extensions.Configuration;

namespace MinimalLambda.Testing.UnitTests;

public class NoEventLambdaTests : IClassFixture<LambdaApplicationFactory<NoEventLambda>>
{
    private readonly LambdaTestServer _server;

    public NoEventLambdaTests(LambdaApplicationFactory<NoEventLambda> factory)
    {
        factory.WithCancellationToken(TestContext.Current.CancellationToken);
        _server = factory.TestServer;
    }

    [Fact]
    public async Task NoEvent_ReturnsExpectedValue()
    {
        var response = await _server.InvokeNoEventAsync<NoEventLambdaResponse>(
            TestContext.Current.CancellationToken
        );

        response.Should().NotBeNull();
        response.WasSuccess.Should().BeTrue();
        response.Response.Should().NotBeNull();
        response.Response.Message.Should().Be("Hello World!");
    }

    [Fact]
    public async Task NoEvent_ConfigurationCanBeOverwritten()
    {
        var factory = new LambdaApplicationFactory<NoEventLambda>()
            .WithCancellationToken(TestContext.Current.CancellationToken)
            .WithHostBuilder(builder =>
                builder.ConfigureAppConfiguration(
                    (_, config) =>
                        config.AddInMemoryCollection(
                            new Dictionary<string, string> { ["MESSAGE"] = "Hello Mars!" }!
                        )
                )
            );

        var response = await factory.TestServer.InvokeNoEventAsync<NoEventLambdaResponse>(
            TestContext.Current.CancellationToken
        );

        response.Should().NotBeNull();
        response.WasSuccess.Should().BeTrue();
        response.Response.Should().NotBeNull();
        response.Response.Message.Should().Be("Hello Mars!");
    }
}
