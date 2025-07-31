using Amazon.Lambda.Core;
using Lambda.Host;
using Lambda.Host.Example.HelloWorld;
using Microsoft.Extensions.DependencyInjection;

var builder = LambdaApplication.CreateBuilder();

builder.Services.AddSingleton<IService, Service>();

builder.Services.AddKeyedSingleton<IService>(
    "key",
    (serviceProvider, _) => serviceProvider.GetRequiredService<IService>()
);

// builder.Services.AddLambdaService();

var lambda = builder.Build();

lambda.MapHandler(
    IService ([Request] string input, IService service) =>
    {
        if (input == "other")
        {
            return new Service();
        }

        return new OtherService();
    }
);

lambda.MapHandler(
    ([Request] string input, IService service) =>
    {
        if (input == "other")
        {
            return "new Service()";
        }

        return "new OtherService()";
    }
);

lambda.MapHandler(
    IService ([Request] string input, IService service) =>
        input == "other" ? new Service() : new OtherService()
);

lambda.MapHandler(
    (Func<string, IService, IService>)(
        ([Request] string input, IService service) =>
        {
            if (input == "other")
            {
                return new Service();
            }

            return new OtherService();
        }
    )
);

lambda.MapHandler(
    ([Request] string input, IService service) =>
    {
        return new OtherService();
    }
);

lambda.MapHandler(string ([Request] string? input, IService service) => "hello world");

lambda.MapHandler(([Request] string? input) => "hello world");

lambda.MapHandler(HandlerFactory.Handler);

await lambda.StartAsync();

public class Service : IService
{
    public Task<string> GetMessage() => Task.FromResult("hello world");
}

public class OtherService : IService
{
    public Task<string> GetMessage() => Task.FromResult("hello world");
}

public static class HandlerFactory
{
    public static async void Handler(
        [Request] string input,
        ILambdaContext context,
        [FromKeyedServices("key")] IService service
    ) { }
}
