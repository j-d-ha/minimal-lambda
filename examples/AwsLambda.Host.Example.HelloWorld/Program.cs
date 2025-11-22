using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using AwsLambda.Host.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = LambdaApplication.CreateBuilder();

builder.Services.AddSingleton<IService, Service>();

var lambda = builder.Build();

lambda.UseMiddleware(
    async (context, next) =>
    {
        Console.WriteLine("[Middleware 1] Before");
        await next(context);
        Console.WriteLine("[Middleware 1] After");
    }
);

lambda.MapHandler(
    Stream ([Event] Stream request, IService service) =>
    {
        var @event = JsonSerializer.Deserialize<Request>(request);

        var response = new Response(service.GetMessage(@event?.Name ?? "EMPTY"));

        var outputStream = new MemoryStream();
        JsonSerializer.Serialize(outputStream, response);
        outputStream.Position = 0;
        return outputStream;
    }
);

await lambda.RunAsync();

internal record Response([property: JsonPropertyName("message")] string Message);

internal record Request([property: JsonPropertyName("name")] string Name);

internal interface IService
{
    string GetMessage(string name);
}

internal class Service : IService
{
    public string GetMessage(string name) => $"hello {name}";
}
