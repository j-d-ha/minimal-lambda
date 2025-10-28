using System;
using AwsLambda.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = LambdaApplication.CreateBuilder();

builder.Services.Configure<HostOptions>(options =>
{
    options.ShutdownTimeout = TimeSpan.FromSeconds(2);
});

var lambda = builder.Build();

lambda.UseClearLambdaOutputFormatting();

lambda.MapHandler(() => new Response("Hello world"));

await lambda.RunAsync();

internal record Response(string Message);
