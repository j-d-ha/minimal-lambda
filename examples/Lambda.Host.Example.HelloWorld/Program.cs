using System;
using Lambda.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = LambdaApplication.CreateBuilder();
builder.Services.AddSingleton<IService, Service>();
var lambda = builder.Build();

lambda.MapHandler(
    (Func<IService, string, string>)(
        (service, input) =>
        {
            Console.WriteLine("hello world");
            return service.GetMessage();
        }
    )
);

await lambda.RunAsync();

public interface IService
{
    string GetMessage();
}

public class Service : IService
{
    public string GetMessage() => "hello world";
}
