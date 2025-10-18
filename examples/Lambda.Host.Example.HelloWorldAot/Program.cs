using System.Text.Json.Serialization;
using Amazon.Lambda.Serialization.SystemTextJson;
using Lambda.Host;
using Microsoft.Extensions.Hosting;

var builder = LambdaApplication.CreateBuilder();

builder.Services.AddLambdaHostedService<MyHost>();

builder.Services.ConfigureLambdaHost(settings =>
    settings.LambdaSerializer = new SourceGeneratorLambdaJsonSerializer<SerializerContext>()
);

var lambda = builder.Build();

lambda.MapHandler(() => "hello world");

await lambda.RunAsync();

[LambdaHost]
public partial class MyHost : LambdaHostedService;

[JsonSerializable(typeof(string))]
public partial class SerializerContext : JsonSerializerContext;
