using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MinimalLambda.Builder;

var builder = LambdaApplication.CreateBuilder();

builder.Configuration.AddInMemoryCollection(
    new Dictionary<string, string> { ["MESSAGE"] = "Hello World!" }!);

await using var lambda = builder.Build();

lambda.MapHandler((IConfiguration configuration) => new NoEventLambdaResponse(
    configuration["MESSAGE"] ?? "NOT_FOUND",
    DateTime.UtcNow));

await lambda.RunAsync();

public class NoEventLambda;

internal record NoEventLambdaResponse(string Message, DateTime TimestampUtc);
