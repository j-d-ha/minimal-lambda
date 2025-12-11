using System;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MinimalLambda.Builder;

var builder = LambdaApplication.CreateBuilder();

builder.Services.AddLambdaSerializerWithContext<SerializerContext>();

var lambda = builder.Build();

lambda.UseClearLambdaOutputFormatting();

lambda.UseMiddleware(
    async (context, next) =>
    {
        Console.WriteLine("[Middleware 1] Before");
        await next(context);
        Console.WriteLine("[Middleware 1] After");
    }
);

lambda.UseMiddleware(
    async (context, next) =>
    {
        Console.WriteLine("[Middleware 2] Before");
        await next(context);
        Console.WriteLine("[Middleware 2] After");
    }
);

lambda.MapHandler(
    ([Event] string input) =>
    {
        Console.WriteLine("hello world from aot");
        return "hello world from aot";
    }
);

await lambda.RunAsync();

[JsonSerializable(typeof(string))]
public partial class SerializerContext : JsonSerializerContext;
