using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace AwsLambda.Host;

/// <summary>
///     Overloads for <see cref="ILambdaApplication.OnInit(LambdaInitDelegate)" /> that support
///     automatic dependency injection for initialization handlers.
/// </summary>
/// <remarks>
///     Source generation creates the wiring code to resolve handler dependencies, using
///     compile-time interceptors to replace the calls. Instead of using the base delegate, declare
///     handler parameters to be automatically injected from the dependency injection container. A
///     scope is created for each handler invocation, and the container is disposed of after the
///     handler returns. If a boolean return value is returned, it will be used to determine whether
///     the Function Init phase should continue, with <c>false</c> from any Init handler aborting the
///     startup of the Function.
/// </remarks>
/// <example>
///     <code>
///     lambda.OnInit(
///         async (ILogger logger, IService service, CancellationToken ct) =>
///         {
///             logger.LogInformation("Initializing");
///             if (!await service.TryLoadDataIntoCache(ct))
///             {
///                 logger.LogError("Failed to load data into cache");
///                 return false;
///             }
///             return true;
///         }
///     );
///     </code>
/// </example>
[ExcludeFromCodeCoverage]
public static class OnInitLambdaApplicationExtensions
{
    /// <summary>
    ///     Registers an initialization handler that will be run during the AWS Lambda Function Init
    ///     phase, before any handler invocation.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Source generation creates the wiring code to resolve handler dependencies, using
    ///         compile-time interceptors to replace the calls. Dependencies are scoped per handler. If a
    ///         CancellationToken is requested, it will be cancelled once the Init timeout has expired. See
    ///         <see cref="LambdaHostOptions.InitTimeout" /> for details.
    ///     </para>
    ///     <para>
    ///         The handler can be synchronous or asynchronous; async handlers are awaited before the
    ///         Function Init phase continues.
    ///     </para>
    ///     <para>
    ///         To control startup, the handler can return a boolean value: return <c>false</c> to abort
    ///         startup and trigger shutdown, or <c>true</c> to continue. Only boolean return values are
    ///         evaluated; any other return value or no return value is treated as <c>true</c> and ignored.
    ///     </para>
    /// </remarks>
    /// <note>
    ///     Return a boolean value only if you need to control the startup lifecycle. Return <c>false</c>
    ///     to abort startup.
    /// </note>
    /// <param name="application">The Lambda application.</param>
    /// <param name="handler">An initialization handler function.</param>
    /// <returns>The current <see cref="ILambdaApplication" /> instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if called at runtime; this exception is
    ///     unreachable as this method is intercepted by the source generator code at compile time.
    /// </exception>
    /// <seealso cref="LambdaInitDelegate" />
    /// <seealso cref="ILambdaApplication.OnInit(LambdaInitDelegate)" />
    public static ILambdaApplication OnInit(this ILambdaApplication application, Delegate handler)
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }
}
