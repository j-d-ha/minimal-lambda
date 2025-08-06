using Lambda.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = LambdaApplication.CreateBuilder();
builder.Services.AddSingleton<IService, Service>();
var lambda = builder.Build();

lambda.MapHandler(([Request] string? input, IService service) => service.GetMessage());

await lambda.RunAsync();

public interface IService
{
    string? GetMessage();
}

public class Service : IService
{
    public string? GetMessage() => "hello world";
}
