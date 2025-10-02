using System.Threading;
using Amazon.Lambda.Core;
using Lambda.Host;
using Microsoft.Extensions.Hosting;

var builder = LambdaApplication.CreateBuilder();

var lambda = builder.Build();

lambda.MapHandler((CancellationToken ct, ILambdaContext ctx) => "hello world");

await lambda.RunAsync();
