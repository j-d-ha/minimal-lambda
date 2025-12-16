using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MinimalLambda.Builder;

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

lambda.OnInit(() =>
{
    Console.WriteLine("Initializing...");
    return Task.FromResult(true);
});

lambda.OnInit(
    async Task<bool> (ILogger<Program> logger, CancellationToken cancellationToken) =>
    {
        var stopwatch = Stopwatch.StartNew();
        while (!cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation(
                "Waiting for init to timeout. {ElapsedMilliseconds}ms elapsed",
                stopwatch.ElapsedMilliseconds
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
    (CancellationToken token) =>
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
