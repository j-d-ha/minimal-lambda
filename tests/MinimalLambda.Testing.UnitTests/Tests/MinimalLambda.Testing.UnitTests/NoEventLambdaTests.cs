namespace MinimalLambda.Testing.UnitTests;

public class NoEventLambdaTests : IClassFixture<LambdaApplicationFactory<NoEventLambda>>
{
    private readonly LambdaTestServer _server;

    public NoEventLambdaTests(LambdaApplicationFactory<NoEventLambda> factory)
    {
        factory.WithCancelationToken(TestContext.Current.CancellationToken);
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
}
