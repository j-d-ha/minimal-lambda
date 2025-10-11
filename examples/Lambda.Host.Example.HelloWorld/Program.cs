using Lambda.Host;
using Microsoft.Extensions.Hosting;

var builder = LambdaApplication.CreateBuilder();

var lambda = builder.Build();

lambda.MapHandler(() => "hello world");

await lambda.RunAsync();
