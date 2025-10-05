using Amazon.Lambda.Core;
using Lambda.Host;
using Microsoft.Extensions.Hosting;

var builder = LambdaApplication.CreateBuilder();

var lambda = builder.Build();

lambda.MapHandler((ILambdaContext ctx, ILambdaContext ctx2) => "hello world");

await lambda.RunAsync();
