using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace AwsLambda.Host;

/// <summary>
///     Overloads for <see cref="ILambdaApplication.OnShutdown(LambdaShutdownDelegate)" /> that support
///     automatic dependency injection
///     for shutdown handlers with zero to sixteen parameters.
/// </summary>
/// <remarks>
///     Source generation creates the wiring code to resolve handler dependencies, using compile-time
///     interceptors to replace the calls. Instead of using the base delegate, declare handler
///     parameters to be automatically injected
///     from the dependency injection container. A scope is created for each handler invocation, and
///     the container is disposed of
///     after the handler returns.
/// </remarks>
/// <example>
///     <code>
///         lambda.OnShutdown(async (ILogger logger, DbContext database) =>
///         {
///             logger.LogInformation("Shutting down");
///             await database.FlushAsync();
///         });
///     </code>
/// </example>
[ExcludeFromCodeCoverage]
public static class OnShutdownLambdaApplicationExtensions
{
    /// <summary>
    ///     Registers a shutdown handler that will be run when the Lambda runtime shuts down.
    /// </summary>
    /// <remarks>
    ///     Source generation creates the wiring code to resolve handler dependencies, using compile-time
    ///     interceptors to replace the calls. Dependencies are scoped per handler. If a CancellationToken
    ///     is requested, it will be cancelled before the Lambda runtime forces shutdown.
    /// </remarks>
    /// <note>
    ///     Shutdown logic should execute quickly as time is minimal before forced termination.
    /// </note>
    /// <param name="application">The Lambda application.</param>
    /// <param name="handler">An asynchronous handler function.</param>
    /// <returns>The current <see cref="ILambdaApplication" /> instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if called at runtime; this exception is unreachable as this method is intercepted by the
    ///     source generator code at compile time.
    /// </exception>
    public static ILambdaApplication OnShutdown(
        this ILambdaApplication application,
        Func<Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }

    /// <inheritdoc cref="OnShutdown(ILambdaApplication, Func{Task})"/>
    /// <typeparam name="T1">The type of the first handler parameter.</typeparam>
    public static ILambdaApplication OnShutdown<T1>(
        this ILambdaApplication application,
        Func<T1, Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }

    /// <inheritdoc cref="OnShutdown(ILambdaApplication, Func{Task})"/>
    /// <typeparam name="T1">The type of the first handler parameter.</typeparam>
    /// <typeparam name="T2">The type of the second handler parameter.</typeparam>
    public static ILambdaApplication OnShutdown<T1, T2>(
        this ILambdaApplication application,
        Func<T1, T2, Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }

    /// <inheritdoc cref="OnShutdown(ILambdaApplication, Func{Task})"/>
    /// <typeparam name="T1">The type of the first handler parameter.</typeparam>
    /// <typeparam name="T2">The type of the second handler parameter.</typeparam>
    /// <typeparam name="T3">The type of the third handler parameter.</typeparam>
    public static ILambdaApplication OnShutdown<T1, T2, T3>(
        this ILambdaApplication application,
        Func<T1, T2, T3, Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }

    /// <inheritdoc cref="OnShutdown(ILambdaApplication, Func{Task})"/>
    /// <typeparam name="T1">The type of the first handler parameter.</typeparam>
    /// <typeparam name="T2">The type of the second handler parameter.</typeparam>
    /// <typeparam name="T3">The type of the third handler parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth handler parameter.</typeparam>
    public static ILambdaApplication OnShutdown<T1, T2, T3, T4>(
        this ILambdaApplication application,
        Func<T1, T2, T3, T4, Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }

    /// <inheritdoc cref="OnShutdown(ILambdaApplication, Func{Task})"/>
    /// <typeparam name="T1">The type of the first handler parameter.</typeparam>
    /// <typeparam name="T2">The type of the second handler parameter.</typeparam>
    /// <typeparam name="T3">The type of the third handler parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth handler parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth handler parameter.</typeparam>
    public static ILambdaApplication OnShutdown<T1, T2, T3, T4, T5>(
        this ILambdaApplication application,
        Func<T1, T2, T3, T4, T5, Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }

    /// <inheritdoc cref="OnShutdown(ILambdaApplication, Func{Task})"/>
    /// <typeparam name="T1">The type of the first handler parameter.</typeparam>
    /// <typeparam name="T2">The type of the second handler parameter.</typeparam>
    /// <typeparam name="T3">The type of the third handler parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth handler parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth handler parameter.</typeparam>
    /// <typeparam name="T6">The type of the sixth handler parameter.</typeparam>
    public static ILambdaApplication OnShutdown<T1, T2, T3, T4, T5, T6>(
        this ILambdaApplication application,
        Func<T1, T2, T3, T4, T5, T6, Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }

    /// <inheritdoc cref="OnShutdown(ILambdaApplication, Func{Task})"/>
    /// <typeparam name="T1">The type of the first handler parameter.</typeparam>
    /// <typeparam name="T2">The type of the second handler parameter.</typeparam>
    /// <typeparam name="T3">The type of the third handler parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth handler parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth handler parameter.</typeparam>
    /// <typeparam name="T6">The type of the sixth handler parameter.</typeparam>
    /// <typeparam name="T7">The type of the seventh handler parameter.</typeparam>
    public static ILambdaApplication OnShutdown<T1, T2, T3, T4, T5, T6, T7>(
        this ILambdaApplication application,
        Func<T1, T2, T3, T4, T5, T6, T7, Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }

    /// <inheritdoc cref="OnShutdown(ILambdaApplication, Func{Task})"/>
    /// <typeparam name="T1">The type of the first handler parameter.</typeparam>
    /// <typeparam name="T2">The type of the second handler parameter.</typeparam>
    /// <typeparam name="T3">The type of the third handler parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth handler parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth handler parameter.</typeparam>
    /// <typeparam name="T6">The type of the sixth handler parameter.</typeparam>
    /// <typeparam name="T7">The type of the seventh handler parameter.</typeparam>
    /// <typeparam name="T8">The type of the eighth handler parameter.</typeparam>
    public static ILambdaApplication OnShutdown<T1, T2, T3, T4, T5, T6, T7, T8>(
        this ILambdaApplication application,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }

    /// <inheritdoc cref="OnShutdown(ILambdaApplication, Func{Task})"/>
    /// <typeparam name="T1">The type of the first handler parameter.</typeparam>
    /// <typeparam name="T2">The type of the second handler parameter.</typeparam>
    /// <typeparam name="T3">The type of the third handler parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth handler parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth handler parameter.</typeparam>
    /// <typeparam name="T6">The type of the sixth handler parameter.</typeparam>
    /// <typeparam name="T7">The type of the seventh handler parameter.</typeparam>
    /// <typeparam name="T8">The type of the eighth handler parameter.</typeparam>
    /// <typeparam name="T9">The type of the ninth handler parameter.</typeparam>
    public static ILambdaApplication OnShutdown<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        this ILambdaApplication application,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }

    /// <inheritdoc cref="OnShutdown(ILambdaApplication, Func{Task})"/>
    /// <typeparam name="T1">The type of the first handler parameter.</typeparam>
    /// <typeparam name="T2">The type of the second handler parameter.</typeparam>
    /// <typeparam name="T3">The type of the third handler parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth handler parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth handler parameter.</typeparam>
    /// <typeparam name="T6">The type of the sixth handler parameter.</typeparam>
    /// <typeparam name="T7">The type of the seventh handler parameter.</typeparam>
    /// <typeparam name="T8">The type of the eighth handler parameter.</typeparam>
    /// <typeparam name="T9">The type of the ninth handler parameter.</typeparam>
    /// <typeparam name="T10">The type of the tenth handler parameter.</typeparam>
    public static ILambdaApplication OnShutdown<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        this ILambdaApplication application,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }

    /// <inheritdoc cref="OnShutdown(ILambdaApplication, Func{Task})"/>
    /// <typeparam name="T1">The type of the first handler parameter.</typeparam>
    /// <typeparam name="T2">The type of the second handler parameter.</typeparam>
    /// <typeparam name="T3">The type of the third handler parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth handler parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth handler parameter.</typeparam>
    /// <typeparam name="T6">The type of the sixth handler parameter.</typeparam>
    /// <typeparam name="T7">The type of the seventh handler parameter.</typeparam>
    /// <typeparam name="T8">The type of the eighth handler parameter.</typeparam>
    /// <typeparam name="T9">The type of the ninth handler parameter.</typeparam>
    /// <typeparam name="T10">The type of the tenth handler parameter.</typeparam>
    /// <typeparam name="T11">The type of the eleventh handler parameter.</typeparam>
    public static ILambdaApplication OnShutdown<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
        this ILambdaApplication application,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }

    /// <inheritdoc cref="OnShutdown(ILambdaApplication, Func{Task})"/>
    /// <typeparam name="T1">The type of the first handler parameter.</typeparam>
    /// <typeparam name="T2">The type of the second handler parameter.</typeparam>
    /// <typeparam name="T3">The type of the third handler parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth handler parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth handler parameter.</typeparam>
    /// <typeparam name="T6">The type of the sixth handler parameter.</typeparam>
    /// <typeparam name="T7">The type of the seventh handler parameter.</typeparam>
    /// <typeparam name="T8">The type of the eighth handler parameter.</typeparam>
    /// <typeparam name="T9">The type of the ninth handler parameter.</typeparam>
    /// <typeparam name="T10">The type of the tenth handler parameter.</typeparam>
    /// <typeparam name="T11">The type of the eleventh handler parameter.</typeparam>
    /// <typeparam name="T12">The type of the twelfth handler parameter.</typeparam>
    public static ILambdaApplication OnShutdown<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
        this ILambdaApplication application,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }

    /// <inheritdoc cref="OnShutdown(ILambdaApplication, Func{Task})"/>
    /// <typeparam name="T1">The type of the first handler parameter.</typeparam>
    /// <typeparam name="T2">The type of the second handler parameter.</typeparam>
    /// <typeparam name="T3">The type of the third handler parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth handler parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth handler parameter.</typeparam>
    /// <typeparam name="T6">The type of the sixth handler parameter.</typeparam>
    /// <typeparam name="T7">The type of the seventh handler parameter.</typeparam>
    /// <typeparam name="T8">The type of the eighth handler parameter.</typeparam>
    /// <typeparam name="T9">The type of the ninth handler parameter.</typeparam>
    /// <typeparam name="T10">The type of the tenth handler parameter.</typeparam>
    /// <typeparam name="T11">The type of the eleventh handler parameter.</typeparam>
    /// <typeparam name="T12">The type of the twelfth handler parameter.</typeparam>
    /// <typeparam name="T13">The type of the thirteenth handler parameter.</typeparam>
    public static ILambdaApplication OnShutdown<
        T1,
        T2,
        T3,
        T4,
        T5,
        T6,
        T7,
        T8,
        T9,
        T10,
        T11,
        T12,
        T13
    >(
        this ILambdaApplication application,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }

    /// <inheritdoc cref="OnShutdown(ILambdaApplication, Func{Task})"/>
    /// <typeparam name="T1">The type of the first handler parameter.</typeparam>
    /// <typeparam name="T2">The type of the second handler parameter.</typeparam>
    /// <typeparam name="T3">The type of the third handler parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth handler parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth handler parameter.</typeparam>
    /// <typeparam name="T6">The type of the sixth handler parameter.</typeparam>
    /// <typeparam name="T7">The type of the seventh handler parameter.</typeparam>
    /// <typeparam name="T8">The type of the eighth handler parameter.</typeparam>
    /// <typeparam name="T9">The type of the ninth handler parameter.</typeparam>
    /// <typeparam name="T10">The type of the tenth handler parameter.</typeparam>
    /// <typeparam name="T11">The type of the eleventh handler parameter.</typeparam>
    /// <typeparam name="T12">The type of the twelfth handler parameter.</typeparam>
    /// <typeparam name="T13">The type of the thirteenth handler parameter.</typeparam>
    /// <typeparam name="T14">The type of the fourteenth handler parameter.</typeparam>
    public static ILambdaApplication OnShutdown<
        T1,
        T2,
        T3,
        T4,
        T5,
        T6,
        T7,
        T8,
        T9,
        T10,
        T11,
        T12,
        T13,
        T14
    >(
        this ILambdaApplication application,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }

    /// <inheritdoc cref="OnShutdown(ILambdaApplication, Func{Task})"/>
    /// <typeparam name="T1">The type of the first handler parameter.</typeparam>
    /// <typeparam name="T2">The type of the second handler parameter.</typeparam>
    /// <typeparam name="T3">The type of the third handler parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth handler parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth handler parameter.</typeparam>
    /// <typeparam name="T6">The type of the sixth handler parameter.</typeparam>
    /// <typeparam name="T7">The type of the seventh handler parameter.</typeparam>
    /// <typeparam name="T8">The type of the eighth handler parameter.</typeparam>
    /// <typeparam name="T9">The type of the ninth handler parameter.</typeparam>
    /// <typeparam name="T10">The type of the tenth handler parameter.</typeparam>
    /// <typeparam name="T11">The type of the eleventh handler parameter.</typeparam>
    /// <typeparam name="T12">The type of the twelfth handler parameter.</typeparam>
    /// <typeparam name="T13">The type of the thirteenth handler parameter.</typeparam>
    /// <typeparam name="T14">The type of the fourteenth handler parameter.</typeparam>
    /// <typeparam name="T15">The type of the fifteenth handler parameter.</typeparam>
    public static ILambdaApplication OnShutdown<
        T1,
        T2,
        T3,
        T4,
        T5,
        T6,
        T7,
        T8,
        T9,
        T10,
        T11,
        T12,
        T13,
        T14,
        T15
    >(
        this ILambdaApplication application,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }

    /// <inheritdoc cref="OnShutdown(ILambdaApplication, Func{Task})"/>
    /// <typeparam name="T1">The type of the first handler parameter.</typeparam>
    /// <typeparam name="T2">The type of the second handler parameter.</typeparam>
    /// <typeparam name="T3">The type of the third handler parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth handler parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth handler parameter.</typeparam>
    /// <typeparam name="T6">The type of the sixth handler parameter.</typeparam>
    /// <typeparam name="T7">The type of the seventh handler parameter.</typeparam>
    /// <typeparam name="T8">The type of the eighth handler parameter.</typeparam>
    /// <typeparam name="T9">The type of the ninth handler parameter.</typeparam>
    /// <typeparam name="T10">The type of the tenth handler parameter.</typeparam>
    /// <typeparam name="T11">The type of the eleventh handler parameter.</typeparam>
    /// <typeparam name="T12">The type of the twelfth handler parameter.</typeparam>
    /// <typeparam name="T13">The type of the thirteenth handler parameter.</typeparam>
    /// <typeparam name="T14">The type of the fourteenth handler parameter.</typeparam>
    /// <typeparam name="T15">The type of the fifteenth handler parameter.</typeparam>
    /// <typeparam name="T16">The type of the sixteenth handler parameter.</typeparam>
    public static ILambdaApplication OnShutdown<
        T1,
        T2,
        T3,
        T4,
        T5,
        T6,
        T7,
        T8,
        T9,
        T10,
        T11,
        T12,
        T13,
        T14,
        T15,
        T16
    >(
        this ILambdaApplication application,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }
}
