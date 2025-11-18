using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AwsLambda.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = LambdaApplication.CreateBuilder();

builder.Services.ConfigureLambdaHostOptions(options =>
{
    options.InitTimeout = TimeSpan.FromSeconds(5);
    options.ShutdownDuration = TimeSpan.FromSeconds(3);
    options.ShutdownDurationBuffer = TimeSpan.FromSeconds(1);
    options.ClearLambdaOutputFormatting = true;
});

var lambda = builder.Build();

lambda.UseClearLambdaOutputFormatting();

lambda.OnInit(
    Task<bool> (IServiceCollection services, CancellationToken cancellationToken) =>
    {
        Console.WriteLine("Initializing...");
        return Task.FromResult(true);
    }
);

lambda.OnInit(
    async Task<bool> (IServiceCollection services, CancellationToken cancellationToken) =>
    {
        var stopwatch = Stopwatch.StartNew();
        while (!cancellationToken.IsCancellationRequested)
        {
            Console.WriteLine(
                $"Waiting for init to timeout. {stopwatch.ElapsedMilliseconds}ms elapsed"
            );
            try
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
            }
            catch
            {
                // ignored
            }
        }

        return true;
    }
);

lambda.MapHandler(() =>
{
    Console.WriteLine("Hello world");
    return new Response("Hello world");
});

lambda.OnShutdown(
    (IServiceCollection services, CancellationToken token) =>
    {
        Console.WriteLine("Shutting down...");
        return Task.CompletedTask;
    }
);

lambda.OnShutdown(
    Task (IService service) =>
    {
        Console.WriteLine(service?.GetMessage());
        return Task.CompletedTask;
    }
);

lambda.OnShutdown(Task () => Task.CompletedTask);

await lambda.RunAsync();

internal record Response(string Message);

public interface IService
{
    string GetMessage();
}

public class Service : IService
{
    public string GetMessage() => $"Message from {nameof(Service)} derived from {nameof(IService)}";
}
