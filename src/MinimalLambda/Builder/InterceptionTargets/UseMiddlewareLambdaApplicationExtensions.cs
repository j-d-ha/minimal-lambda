using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace MinimalLambda.Builder;

[ExcludeFromCodeCoverage]
public static class UseMiddlewareLambdaApplicationExtensions
{
    public static ILambdaOnInitBuilder UseMiddleware<T>(this ILambdaOnInitBuilder _)
        where T : ILambdaMiddleware
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }
}
