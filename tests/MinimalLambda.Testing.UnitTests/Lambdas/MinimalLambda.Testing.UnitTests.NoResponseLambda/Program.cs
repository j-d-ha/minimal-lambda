using Microsoft.Extensions.Hosting;
using MinimalLambda.Builder;

var builder = LambdaApplication.CreateBuilder();

await using var lambda = builder.Build();

lambda.MapHandler(([FromEvent] NoResponseLambdaRequest request) => { });

await lambda.RunAsync();

public class NoResponseLambda;

internal record NoResponseLambdaRequest(string Name);
