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
        await client.WaitForNextRequestAsync();
        var response = await client.InvokeAsync<string, string>("Jonas");
        Assert.True(response.WasSuccess);
        Assert.NotNull(response);
        Assert.Equal("Hello Jonas!", response.Response);
    }
}
