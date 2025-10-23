using System;
using System.Threading.Tasks;
using Lambda.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = LambdaApplication.CreateBuilder();
builder.Services.AddSingleton<IService, Service>();
var lambda = builder.Build();

lambda.MapHandler(
    (Action<string, IService>)(
        ([Event] string input, IService service) =>
        {
            Console.WriteLine("hello world");
        }
    )
);

await lambda.RunAsync();

public interface IService
{
    Task<string> GetMessage();
}

public class Service : IService
{
    public Task<string> GetMessage() => Task.FromResult("hello world");
}
