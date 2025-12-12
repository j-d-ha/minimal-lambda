using Microsoft.Extensions.Hosting;
using MinimalLambda.Builder;

var builder = LambdaApplication.CreateBuilder();

await using var lambda = builder.Build();

lambda.MapHandler(() => new NoEventLambdaResponse("Hello World!", DateTime.UtcNow));

await lambda.RunAsync();

public class NoEventLambda;

internal record NoEventLambdaResponse(string Message, DateTime TimestampUtc);
