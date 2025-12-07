using AwsLambda.Host.Testing;
using JetBrains.Annotations;
using Xunit;

namespace Lambda.Host.Example.HelloWorld;

[TestSubject(typeof(Program))]
public class LambdaHostTest
{
    [Fact]
    public async Task LambdaHost_CanStartWithoutError()
    {
        await using var factory = new WebApplicationFactory<Program>();

        var client = factory.CreateClient();
        // No need to wait for next request - server handles this automatically
        var response = await client.InvokeAsync<string, string>(
            "Jonas",
            TestContext.Current.CancellationToken
        );
        Assert.True(response.WasSuccess);
        Assert.NotNull(response);
        Assert.Equal("Hello Jonas!", response.Response);
    }

    [Fact]
    public async Task LambdaHost_ProcessesConcurrentInvocationsInFifoOrder()
    {
        await using var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        // Launch 5 concurrent invocations
        var tasks = Enumerable
            .Range(1, 5)
            .Select(i =>
                client.InvokeAsync<string, string>(
                    $"User{i}",
                    TestContext.Current.CancellationToken
                )
            )
            .ToArray();

        var responses = await Task.WhenAll(tasks);

        // All should complete successfully
        Assert.All(responses, r => Assert.True(r.WasSuccess));
        Assert.Equal("Hello User1!", responses[0].Response);
        Assert.Equal("Hello User2!", responses[1].Response);
        Assert.Equal("Hello User3!", responses[2].Response);
        Assert.Equal("Hello User4!", responses[3].Response);
        Assert.Equal("Hello User5!", responses[4].Response);
    }

    [Fact]
    public async Task InvokeAsync_WithInvalidPayload_ReturnsError()
    {
        await using var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        var response = await client.InvokeAsync<string, int>(
            123,
            TestContext.Current.CancellationToken
        );

        Assert.False(response.WasSuccess);
        Assert.NotNull(response.Error);
        Assert.Contains("Json", response.Error!.ErrorType, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InvokeAsync_WithPreCanceledToken_CancelsInvocation()
    {
        await using var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            client.InvokeAsync<string, string>("Jonas", cts.Token)
        );
    }

    [Fact]
    public async Task InvokeAsync_WithZeroTimeout_CancelsInvocation() =>
        await Assert.ThrowsAsync<AggregateException>(async () =>
        {
            await using var factory = new WebApplicationFactory<Program>();
            var client = factory
                .CreateClient()
                .ConfigureOptions(options =>
                    options.InvocationHeaderOptions.ClientWaitTimeout = TimeSpan.Zero
                );

            await client.InvokeAsync<string, string>(
                "Jonas",
                TestContext.Current.CancellationToken
            );
        });
}
