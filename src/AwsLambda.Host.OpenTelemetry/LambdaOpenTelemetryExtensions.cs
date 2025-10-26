using System.Diagnostics;

namespace AwsLambda.Host;

public static class LambdaOpenTelemetryExtensions
{
    /// <summary>
    /// Enables OpenTelemetry tracing for the AWS Lambda handler.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method serves as a no-op placeholder that is intercepted and replaced at compile time
    /// by the AwsLambda.Host source generator. It uses the OpenTelemetry instrumentation provided
    /// by the <see href="https://www.nuget.org/packages/OpenTelemetry.Instrumentation.AWSLambda">OpenTelemetry.Instrumentation.AWSLambda</see>
    /// package to automatically instrument Lambda handler invocations with distributed tracing.
    /// </para>
    /// <para>
    /// When this method is called, the source generator creates an interceptor that:
    /// <list type="bullet">
    ///   <item><description>Intercepts the method call at the call site using C# 11 interceptors</description></item>
    ///   <item><description>Retrieves the configured OpenTelemetry TracerProvider from the dependency injection container</description></item>
    ///   <item><description>Wraps the handler pipeline with tracing middleware</description></item>
    ///   <item><description>Automatically captures traces and exports them to configured providers (e.g., CloudWatch, Jaeger)</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// If this method is called at runtime without being intercepted by the source generator,
    /// it will throw an <see cref="InvalidOperationException"/> as a safety measure.
    /// </para>
    /// </remarks>
    /// <param name="application">The Lambda application instance.</param>
    /// <returns>The same <see cref="ILambdaApplication"/> instance for method chaining.</returns>
    /// <example>
    /// <para>
    /// First, register OpenTelemetry in the dependency injection container using AWS Lambda configurations:
    /// </para>
    /// <code language="csharp">
    /// var builder = LambdaApplication.CreateBuilder();
    ///
    /// builder
    ///     .Services.AddOpenTelemetry()
    ///     .WithTracing(configure =&gt; configure
    ///         .AddAWSLambdaConfigurations()
    ///         .AddConsoleExporter());
    /// </code>
    /// <para>
    /// Then call this method in your Lambda handler setup to enable tracing:
    /// </para>
    /// <code language="csharp">
    /// var lambda = builder.Build();
    ///
    /// lambda.UseOpenTelemetryTracing();
    ///
    /// lambda.MapHandler(([Event] Request request) =&gt; new Response($"Hello {request.Name}!"));
    ///
    /// await lambda.RunAsync();
    ///
    /// record Request(string Name);
    /// record Response(string Message);
    /// </code>
    /// <para>
    /// The source generator creates an interceptor at compile time that transforms the call. The generated
    /// interceptor code looks similar to:
    /// </para>
    /// <code language="csharp">
    /// [InterceptsLocation(...)]
    /// internal static ILambdaApplication UseOpenTelemetryTracingInterceptor(
    ///     this ILambdaApplication application
    /// )
    /// {
    ///     return application.Use(application.Services.GetTracer&lt;global::Request, global::Response&gt;());
    /// }
    /// </code>
    /// <para>
    /// The interceptor retrieves the configured tracer from the service provider
    /// using the actual request and response types defined in your handler (<c>Request</c> and <c>Response</c> in this example).
    /// It injects the tracing middleware into the pipeline, automatically enabling distributed tracing for all handler invocations.
    /// </para>
    /// <para>
    /// The generated interceptor adapts to various handler signatures by calling the appropriate
    /// <see cref="LambdaOpenTelemetryServiceProviderExtensions"/> extension methods:
    /// <list type="bullet">
    ///   <item><description>Handlers with both input and output: <c>services.GetTracer&lt;TRequest, TResponse&gt;()</c></description></item>
    ///   <item><description>Handlers with input only (no response): <c>services.GetTracerNoResponse&lt;TRequest&gt;()</c></description></item>
    ///   <item><description>Handlers with no input (void event): <c>services.GetTracerNoEvent&lt;TResponse&gt;()</c></description></item>
    ///   <item><description>Handlers with neither input nor output: <c>services.GetTracerNoEventNoResponse()</c></description></item>
    /// </list>
    /// </para>
    /// </example>
    public static ILambdaApplication UseOpenTelemetryTracing(this ILambdaApplication application)
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }
}
