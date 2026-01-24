using System;
using Microsoft.Extensions.Hosting;
using MinimalLambda.Builder;

// Create the application builder
var builder = LambdaApplication.CreateBuilder();

// Build the Lambda application
var lambda = builder.Build();

// Map your handler - the event is automatically injected
lambda.MapHandler(([FromEvent] Request request) =>
new Response($"Hello {request.Name}!", DateTime.UtcNow));

// Run the Lambda
await lambda.RunAsync();

internal record Response(string Message, DateTime TimestampUtc);

internal record Request(string Name);
