using Amazon.Lambda.Core;
using Lambda.Host;
using Lambda.Host.Example.HelloWorld;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = LambdaApplication.CreateBuilder();

builder.Services.AddSingleton<IService, Service>();

builder.Services.AddKeyedSingleton<IService>(
    "key",
    (serviceProvider, _) => serviceProvider.GetRequiredService<IService>()
);

// builder.Services.AddLambdaService();

var lambda = builder.Build();

// lambda.MapHandler(
//     async ([Request] string input, IService service) => (await service.GetMessage()).ToUpper()
// );

lambda.MapHandler(HandlerFactory.Handler);

// lambda.MapHandler(
//     IService ([Request] string input, IService service) =>
//     {
//         if (input == "other")
//         {
//             return new Service();
//         }
//
//         return new OtherService();
//     }
// );

// lambda.MapHandler(
//     ([Request] string input, IService service) =>
//     {
//         if (input == "other")
//         {
//             return "new Service()";
//         }
//
//         return "new OtherService()";
//     }
// );

// lambda.MapHandler(
//     IService ([Request] string input, IService service) =>
//         input == "other" ? new Service() : new OtherService()
// );

// lambda.MapHandler(
//     (Func<string, IService, string>)(
//         ([Request] string input, [FromKeyedServices("key")] IService service) =>
//         {
//             Console.WriteLine("hello world");
//
//             return "hello world";
//
//             // if (input == "other")
//             // {
//             //     return new Service();
//             // }
//             //
//             // return new OtherService();
//         }
//     )
// );

// lambda.MapHandler(
//     (Action<string, IService>)(
//         ([Request] string input, IService service) =>
//         {
//             Console.WriteLine("hello world");
//         }
//     )
// );

// lambda.MapHandler(
//     ([Request] string input, IService service) =>
//     {
//         return new OtherService();
//     }
// );

// lambda.MapHandler(string ([Request] string? input, IService service) => "hello world");

// lambda.MapHandler(([Request] string? input) => "hello world");

// static async void Handler(
//     [Request] string input,
//     ILambdaContext context,
//     [FromKeyedServices("key")] IService service
// ) { }

// lambda.MapHandler(Handler);

await lambda.RunAsync();

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
    public static async Task<string> Handler(
        [Request] string input,
        ILambdaContext context,
        [FromKeyedServices("key")] IService service,
        IService otherService
    )
    {
        return await service.GetMessage();
    }
}
