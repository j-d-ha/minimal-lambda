using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace AwsLambda.Host;

[ExcludeFromCodeCoverage]
public static class OnShutdownLambdaApplicationExtensions
{
    public static ILambdaApplication OnShutdown(
        this ILambdaApplication application,
        Func<Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }

    public static ILambdaApplication OnShutdown<T1>(
        this ILambdaApplication application,
        Func<T1, Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }

    public static ILambdaApplication OnShutdown<T1, T2>(
        this ILambdaApplication application,
        Func<T1, T2, Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }

    public static ILambdaApplication OnShutdown<T1, T2, T3>(
        this ILambdaApplication application,
        Func<T1, T2, T3, Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }

    public static ILambdaApplication OnShutdown<T1, T2, T3, T4>(
        this ILambdaApplication application,
        Func<T1, T2, T3, T4, Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }

    public static ILambdaApplication OnShutdown<T1, T2, T3, T4, T5>(
        this ILambdaApplication application,
        Func<T1, T2, T3, T4, T5, Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }

    public static ILambdaApplication OnShutdown<T1, T2, T3, T4, T5, T6>(
        this ILambdaApplication application,
        Func<T1, T2, T3, T4, T5, T6, Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }

    public static ILambdaApplication OnShutdown<T1, T2, T3, T4, T5, T6, T7>(
        this ILambdaApplication application,
        Func<T1, T2, T3, T4, T5, T6, T7, Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }

    public static ILambdaApplication OnShutdown<T1, T2, T3, T4, T5, T6, T7, T8>(
        this ILambdaApplication application,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }

    public static ILambdaApplication OnShutdown<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        this ILambdaApplication application,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }

    public static ILambdaApplication OnShutdown<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        this ILambdaApplication application,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, Task> handler
    )
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }
}
