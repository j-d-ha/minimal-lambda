using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace AwsLambda.Host;

[ExcludeFromCodeCoverage]
public static class OnInitLambdaApplicationExtensions
{
    public static ILambdaApplication OnInit(this ILambdaApplication application, Delegate handler)
    {
        Debug.Fail("This method should have been intercepted at compile time!");
        throw new InvalidOperationException("This method is replaced at compile time.");
    }
}
