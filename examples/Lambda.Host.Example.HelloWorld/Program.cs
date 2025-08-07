using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Lambda.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = LambdaApplication.CreateBuilder();
builder.Services.AddSingleton<IService, Service>();
var lambda = builder.Build();

lambda.MapHandler(
    async ([Request] CustomRequest request, IService service, ILambdaContext context) =>
        new CustomResponse { Result = await service.GetMessage(), Success = true }
);

await lambda.RunAsync();

public class CustomRequest
{
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
}

public class CustomResponse
{
    public string Result { get; set; } = string.Empty;
    public bool Success { get; set; }
}

public interface IService
{
    Task<string> GetMessage();
}

public class Service : IService
{
    public Task<string> GetMessage() => Task.FromResult("hello world");
}
