using Microsoft.Extensions.Hosting;
using MinimalLambda.Builder;

var builder = LambdaApplication.CreateBuilder();

builder.Services.AddScoped<IService, Service>();
builder.Services.AddSingleton<Instrumentation>();
builder.Services.AddSingleton<NameMetrics>();

builder.Services.AddOtel();

var lambda = builder.Build();

lambda.UseOpenTelemetryTracing();

lambda.MapHandler(Function.Handler);

lambda.OnShutdownFlushOpenTelemetry();

await lambda.RunAsync();
