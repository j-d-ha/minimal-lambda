using System.Linq;
using System.Threading.Tasks;
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
}
