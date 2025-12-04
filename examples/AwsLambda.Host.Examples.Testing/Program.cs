using AwsLambda.Host.Builder;
using Microsoft.Extensions.Hosting;

// Create the application builder
var builder = LambdaApplication.CreateBuilder();

// Build the Lambda application
var lambda = builder.Build();

// Map your handler - the event is automatically injected
lambda.MapHandler(([Event] string name) => $"Hello {name}!");

// Run the Lambda
await lambda.RunAsync();
