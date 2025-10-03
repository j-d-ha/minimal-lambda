using System.IO;
using Lambda.Host;
using Microsoft.Extensions.Hosting;

var builder = LambdaApplication.CreateBuilder();

var lambda = builder.Build();

lambda.MapHandler(Stream () => new FileStream("hello.txt", FileMode.Open));

await lambda.RunAsync();
