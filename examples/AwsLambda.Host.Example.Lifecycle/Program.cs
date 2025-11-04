using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AwsLambda.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = LambdaApplication.CreateBuilder();

builder.Services.ConfigureLambdaHost(options =>
{
    options.InitTimeout = TimeSpan.FromSeconds(5);
    options.ShutdownDuration = TimeSpan.FromSeconds(3);
    options.ShutdownDurationBuffer = TimeSpan.FromSeconds(1);
});

builder.Services.AddSingleton<IService, Service>();

var lambda = builder.Build();

lambda.UseClearLambdaOutputFormatting();

lambda.OnInit(
    Task<bool> (services, token) =>
    {
        Console.WriteLine("Initializing...");
        return Task.FromResult(true);
    }
);

lambda.OnInit(
    async Task<bool> (services, cancellationToken) =>
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
    (services, token) =>
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
