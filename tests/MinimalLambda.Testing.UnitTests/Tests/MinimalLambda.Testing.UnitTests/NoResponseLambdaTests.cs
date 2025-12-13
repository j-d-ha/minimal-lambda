using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MinimalLambda.Testing.UnitTests;

public class NoResponseLambdaTests : IClassFixture<LambdaApplicationFactory<NoResponseLambda>>
{
    private readonly LambdaTestServer _server;

    public NoResponseLambdaTests(LambdaApplicationFactory<NoResponseLambda> factory)
    {
        factory.WithCancellationToken(TestContext.Current.CancellationToken);
        _server = factory.TestServer;
    }

    [Fact]
    public async Task NoResponseLambda_ReturnsExpectedValue()
    {
        var response = await _server.InvokeNoResponseAsync<NoResponseLambdaRequest>(
            new NoResponseLambdaRequest("World"),
            TestContext.Current.CancellationToken
        );

        response.WasSuccess.Should().BeTrue();
        response.Should().NotBeNull();
    }

    [Fact]
    public void NoResponseLambda_ServicesIsAccessible()
    {
        var act = () => _server.Services.GetRequiredService<IHostApplicationLifetime>();

        act.Should().NotThrow();
    }

    [Fact]
    public async Task NoResponseLambda_DisposeCanBeCalledMultipleTimes()
    {
        await using var factory =
            new LambdaApplicationFactory<NoResponseLambda>().WithCancellationToken(
                TestContext.Current.CancellationToken
            );

        var act = async () =>
        {
            await factory.TestServer.DisposeAsync();
            await factory.TestServer.DisposeAsync();
        };

        await act.Should().NotThrowAsync();
    }
}
