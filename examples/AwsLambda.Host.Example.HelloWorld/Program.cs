using System;
using AwsLambda.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = LambdaApplication.CreateBuilder();

builder.Services.AddSingleton<IService, Service>();

var lambda = builder.Build();

lambda.UseClearLambdaOutputFormatting();

lambda.MapHandler(
    ([Event] Request request, IService service) =>
    {
        Console.WriteLine("request: " + request.Name);
        return new Response(service.GetMessage(request.Name));
    }
);

lambda.UseMiddleware(
    async (context, next) =>
    {
        Console.WriteLine("middleware 1: before");
        await next(context);
        Console.WriteLine("middleware 1: after");
    }
);

lambda.UseMiddleware(
    async (context, next) =>
    {
        Console.WriteLine("middleware 2: before");
        await next(context);
        Console.WriteLine("middleware 2: after");
    }
);

lambda.OnShutdown(
    async (services, token) =>
    {
        Console.WriteLine("shutdown");
    }
);

await lambda.RunAsync();

internal record Response(string Message);

internal record Request(string Name);

internal interface IService
{
    string GetMessage(string name);
}

internal class Service : IService
{
    public string GetMessage(string name) => $"hello {name}";
}
