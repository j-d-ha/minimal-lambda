using System;
using System.Threading.Tasks;
using AwsLambda.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = LambdaApplication.CreateBuilder();

builder.Services.ConfigureLambdaHost(options =>
{
    options.RuntimeShutdownDuration = TimeSpan.FromSeconds(3);
    options.RuntimeShutdownDurationBuffer = TimeSpan.FromSeconds(1);
});

builder.Services.AddSingleton<IService, Service>();

var lambda = builder.Build();

lambda.UseClearLambdaOutputFormatting();

lambda.MapHandler(() =>
{
    Console.WriteLine("Hello world");
    return new Response("Hello world");
});

lambda.OnShutdown(
    async (services, token) =>
    {
        Console.WriteLine("Shutting down...");
    }
);

lambda.OnShutdown(
    Task (IService service) =>
    {
        Console.WriteLine(service?.GetMessage());
        return Task.CompletedTask;
    }
);

lambda.OnShutdown(Task () =>
{
    return Task.CompletedTask;
});

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
