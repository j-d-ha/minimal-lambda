using System.Text.Json.Serialization;
using Lambda.Host;
using Microsoft.Extensions.Hosting;

var builder = LambdaApplication.CreateBuilder();

builder.UseLambdaHost<MyHost>();

builder.Services.AddLambdaJsonSerializer<SerializerContext>();

var lambda = builder.Build();

lambda.MapHandler(() => "hello world");

await lambda.RunAsync();

[LambdaHost]
public partial class MyHost : LambdaHostedService;

[JsonSerializable(typeof(string))]
public partial class SerializerContext : JsonSerializerContext;
