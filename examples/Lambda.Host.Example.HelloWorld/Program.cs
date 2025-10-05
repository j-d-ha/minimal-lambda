using Lambda.Host;
using Microsoft.Extensions.Hosting;

var builder = LambdaApplication.CreateBuilder();

var lambda = builder.Build();

lambda.MapHandler(([Request] string __input) => __input.ToUpper());

await lambda.RunAsync();
