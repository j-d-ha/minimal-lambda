using System;
using AwsLambda.Host.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Create the application builder
var builder = LambdaApplication.CreateBuilder();

builder.Services.ConfigureLambdaHostOptions(options =>
{
    // options.BootstrapHttpClient = new HttpClient(new LoggingHttpHandler());
    options.ClearLambdaOutputFormatting = true;
    // options.BootstrapOptions.RuntimeApiEndpoint = "localhost:3002";
});

// Build the Lambda application
var lambda = builder.Build();

// Map your handler - the event is automatically injected
lambda.MapHandler(([Event] string name) => $"Hello {name}!");

// Run the Lambda
await lambda.RunAsync();

public partial class Program;

// public class LoggingHttpHandler : DelegatingHandler
// {
//     public LoggingHttpHandler(HttpMessageHandler innerHandler) =>
//         InnerHandler = innerHandler ?? throw new ArgumentNullException(nameof(innerHandler));
//
//     // Convenience constructor for default handler
//     public LoggingHttpHandler()
//         : this(new HttpClientHandler()) { }
//
//     protected override async Task<HttpResponseMessage> SendAsync(
//         HttpRequestMessage request,
//         CancellationToken cancellationToken
//     )
//     {
//         // Log the request
//         Console.WriteLine("========== HTTP REQUEST ==========");
//         Console.WriteLine($"{request.Method} {request.RequestUri}");
//         Console.WriteLine($"Version: {request.Version}");
//
//         Console.WriteLine("\nHeaders:");
//         foreach (var header in request.Headers)
//             Console.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
//
//         if (request.Content != null)
//         {
//             Console.WriteLine("\nContent Headers:");
//             foreach (var header in request.Content.Headers)
//                 Console.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
//
//             // Buffer the content so we can read it without consuming it
//             await request.Content.LoadIntoBufferAsync();
//             var requestBody = await request.Content.ReadAsStringAsync();
//
//             Console.WriteLine("\nBody:");
//             Console.WriteLine(requestBody);
//         }
//
//         // Send the request through the inner handler (via base.SendAsync)
//         var response = await base.SendAsync(request, cancellationToken);
//
//         // Log the response
//         Console.WriteLine("\n========== HTTP RESPONSE ==========");
//         Console.WriteLine($"Status: {(int)response.StatusCode} {response.StatusCode}");
//         Console.WriteLine($"Version: {response.Version}");
//
//         Console.WriteLine("\nHeaders:");
//         foreach (var header in response.Headers)
//             Console.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
//
//         if (response.Content != null)
//         {
//             Console.WriteLine("\nContent Headers:");
//             foreach (var header in response.Content.Headers)
//                 Console.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
//
//             // Read and log response body, then restore it
//             var responseBody = await response.Content.ReadAsStringAsync();
//             Console.WriteLine("\nBody:");
//             Console.WriteLine(responseBody);
//
//             // Restore the content so it can be read again by the caller
//             var originalContentHeaders = response.Content.Headers.ToList();
//             response.Content = new StringContent(
//                 responseBody,
//                 Encoding.UTF8,
//                 response.Content.Headers.ContentType?.MediaType ?? "application/json"
//             );
//
//             // Restore all content headers
//             foreach (var header in originalContentHeaders)
//             {
//                 response.Content.Headers.Remove(header.Key);
//                 response.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
//             }
//         }
//
//         Console.WriteLine("===================================\n");
//
//         return response;
//     }
// }
