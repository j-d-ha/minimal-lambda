using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using OpenTelemetry.Trace;

namespace AwsLambda.Host.Builder;

/// <summary>
///     Provides extension methods for enabling OpenTelemetry tracing in the Lambda invocation
///     pipeline.
/// </summary>
[ExcludeFromCodeCoverage]
public static class MiddlewareOpenTelemetryExtensions
{
    /// <summary>Enables OpenTelemetry tracing for the AWS Lambda handler.</summary>
    /// <remarks>
    ///     <para>
    ///         This method is as a no-op that is intercepted and replaced at compile time It uses the
    ///         OpenTelemetry instrumentation provided by the
    ///         <see href="https://www.nuget.org/packages/OpenTelemetry.Instrumentation.AWSLambda">OpenTelemetry.Instrumentation.AWSLambda</see>
    ///         package to instrument the Lambda handler invocations with distributed tracing.
    ///     </para>
    ///     <para>
    ///         When this method is called, the source generator creates an interceptor that:
    ///         <list type="bullet">
    ///             <item>
    ///                 <description>
    ///                     At startup, pulls an instance of <see cref="TracerProvider" /> from
    ///                     the dependency injection container. This will be used for the lifetime of the
    ///                     Lambda.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     Wraps the handler pipeline with tracing middleware that creates a root
    ///                     span with invocation info.
    ///                 </description>
    ///             </item>
    ///         </list>
    ///     </para>
    ///     <para>
    ///         <b>Middleware Placement:</b> For the most accurate trace data, this method should be
    ///         called at the top of the middleware pipeline, before other middleware where possible. This
    ///         ensures that tracing captures as much of the invocation as possible, including the
    ///         execution time of subsequent middleware components.
    ///     </para>
    ///     <para>
    ///         <b>TracerProvider Registration Required:</b> A <see cref="TracerProvider" /> instance
    ///         must be registered in the dependency injection container before calling this method. If no
    ///         instance is registered, an <see cref="InvalidOperationException" /> will be thrown at
    ///         startup.
    ///     </para>
    /// </remarks>
    /// <param name="application">The <see cref="ILambdaApplication" /> instance.</param>
    /// <returns>The same <see cref="ILambdaApplication" /> instance for method chaining.</returns>
    /// <example>
    ///     <para>
    ///         First, register OpenTelemetry in the dependency injection container using AWS Lambda
    ///         configurations:
    ///     </para>
    ///     <code language="csharp">
    ///     var builder = LambdaApplication.CreateBuilder();
    ///
    ///     builder
    ///         .Services.AddOpenTelemetry()
    ///         .WithTracing(configure =&gt; configure
    ///             .AddAWSLambdaConfigurations()
    ///             .AddConsoleExporter());
    ///     </code>
    ///     <para>Then call this method in your Lambda handler setup to enable tracing:</para>
    ///     <code language="csharp">
    ///     var lambda = builder.Build();
    ///
    ///     lambda.UseOpenTelemetryTracing();
    ///
    ///     lambda.MapHandler(([Event] Request request) =&gt; new Response($"Hello {request.Name}!"));
    ///
    ///     await lambda.RunAsync();
    ///
    ///     record Request(string Name);
    ///     record Response(string Message);
    ///     </code>
    /// </example>
    public static ILambdaInvocationBuilder UseOpenTelemetryTracing(
        this ILambdaInvocationBuilder application
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }
}
