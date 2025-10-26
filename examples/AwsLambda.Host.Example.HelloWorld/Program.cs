using AwsLambda.Host;
using Microsoft.Extensions.Hosting;

var builder = LambdaApplication.CreateBuilder();

var lambda = builder.Build();

lambda.UseOpenTelemetryTracing();

lambda.MapHandler(([Event] Request request) => new Response($"Hello {request.Name}!"));

await lambda.RunAsync();

// {"Name":"jonas"}
record Request(string Name);

record Response(string Message);
