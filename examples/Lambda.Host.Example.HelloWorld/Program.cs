using Amazon.Lambda.Core;
using Lambda.Host;
using Microsoft.Extensions.Hosting;

var builder = LambdaApplication.CreateBuilder<HostedService>();

var lambda = builder.Build();

lambda.MapHandler((ILambdaContext context) => "hello world");

await lambda.RunAsync();

[LambdaHost]
public class HostedService : LambdaHostedService;
