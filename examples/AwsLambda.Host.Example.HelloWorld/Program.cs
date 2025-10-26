using AwsLambda.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Instrumentation.AWSLambda;
using OpenTelemetry.Trace;

var builder = LambdaApplication.CreateBuilder();

builder
    .Services.AddOpenTelemetry()
    .WithTracing(configure => configure.AddAWSLambdaConfigurations().AddConsoleExporter());

var lambda = builder.Build();

lambda.UseClearLambdaOutputFormatting();

lambda.UseOpenTelemetryTracing();

lambda.MapHandler(([Event] Request request) => new Response($"Hello {request.Name}!"));

await lambda.RunAsync();

// {"Name":"john"}
record Request(string Name);

record Response(string Message);
