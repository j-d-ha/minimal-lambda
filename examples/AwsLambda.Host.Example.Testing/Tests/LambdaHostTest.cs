using AwesomeAssertions;
using AwsLambda.Host.Options;
using AwsLambda.Host.Testing;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Lambda.Host.Example.HelloWorld;

[TestSubject(typeof(Program))]
public class LambdaHostTest
{
    [Fact]
    public async Task LambdaHost_CanStartWithoutError()
    {
        await using var factory = new LambdaApplicationFactory<Program>();

        var setup = await factory.Server.StartAsync(TestContext.Current.CancellationToken);

        setup.InitStatus.Should().Be(InitStatus.InitCompleted);

        var response = await factory.Server.InvokeAsync<string, string>(
            "Jonas",
            TestContext.Current.CancellationToken
        );

        response.WasSuccess.Should().BeTrue();
        response.Should().NotBeNull();
        response.Response.Should().Be("Hello Jonas!");
    }

    [Fact]
    public async Task LambdaHost_HandlerReturnsError()
    {
        await using var factory = new LambdaApplicationFactory<Program>();

        var setup = await factory.Server.StartAsync(TestContext.Current.CancellationToken);

        setup.InitStatus.Should().Be(InitStatus.InitCompleted);

        var response = await factory.Server.InvokeAsync<string, string>(
            "",
            TestContext.Current.CancellationToken
        );

        response.WasSuccess.Should().BeFalse();
        response.Error.Should().NotBeNull();
        response.Error?.ErrorMessage.Should().Be("Name is required. (Parameter 'name')");
    }

    [Fact]
    public async Task LambdaHost_CrashesWithBadConfiguration_ThrowsException()
    {
        await using var factory = new LambdaApplicationFactory<Program>().WithHostBuilder(builder =>
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

        await factory.Server.StartAsync(TestContext.Current.CancellationToken);

        var response = await factory.Server.InvokeAsync<string, string>(
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
        await using var factory = new LambdaApplicationFactory<Program>();
        await factory.Server.StartAsync(TestContext.Current.CancellationToken);

        // Launch 5 concurrent invocations
        var tasks = Enumerable
            .Range(1, 5)
            .Select(i =>
                factory.Server.InvokeAsync<string, string>(
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

    // [Fact]
    // public async Task InvokeAsync_WithInvalidPayload_ReturnsError()
    // {
    //     await using var factory = new LambdaApplicationFactory<Program>();
    //     await factory.Server.StartAsync(TestContext.Current.CancellationToken);
    //
    //     var response = await factory.Server.InvokeAsync<string, int>(
    //         123,
    //         TestContext.Current.CancellationToken
    //     );
    //
    //     Assert.False(response.WasSuccess);
    //     Assert.NotNull(response.Error);
    //     Assert.Contains("Json", response.Error!.ErrorType, StringComparison.OrdinalIgnoreCase);
    // }
    //
    // [Fact]
    // public async Task InvokeAsync_WithPreCanceledToken_CancelsInvocation()
    // {
    //     await using var factory = new LambdaApplicationFactory<Program>();
    //     await factory.Server.StartAsync(TestContext.Current.CancellationToken);
    //
    //     using var cts = new CancellationTokenSource();
    //     await cts.CancelAsync();
    //
    //     await Assert.ThrowsAsync<TaskCanceledException>(() =>
    //         factory.Server.InvokeAsync<string, string>("Jonas", cts.Token)
    //     );
    // }
    //
    // [Fact]
    // public async Task InvokeAsync_WithZeroTimeout_CancelsInvocation() =>
    //     await Assert.ThrowsAsync<TaskCanceledException>(async () =>
    //     {
    //         await using var factory = new LambdaApplicationFactory<Program>();
    //         await factory.Server.StartAsync(TestContext.Current.CancellationToken);
    //
    //         var options = new LambdaClientOptions();
    //         options.InvocationHeaderOptions.ClientWaitTimeout = TimeSpan.Zero;
    //
    //         await factory.Server.InvokeAsync<string, string>(
    //             "Jonas",
    //             options,
    //             TestContext.Current.CancellationToken
    //         );
    //     });
    //
    // [Fact]
    // public async Task StartAsync_WithFailingInit_ReturnsInitError()
    // {
    //     // This test verifies that when OnInit returns false (as configured in Program.cs),
    //     // the runtime posts to /runtime/init/error and StartAsync returns InitResponse with
    // error
    //     await using var factory = new LambdaApplicationFactory<Program>();
    //
    //     using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
    //     var initResponse = await factory.Server.StartAsync(cts.Token);
    //
    //     Assert.False(initResponse.InitSuccess);
    //     Assert.NotNull(initResponse.Error);
    //     Assert.Equal(ServerState.Stopped, factory.Server.State);
    // }
}
