namespace Lambda.Host.Middleware;

public static class DefaultMiddleware
{
    public static async Task ClearLambdaOutputFormatting(
        ILambdaHostContext context,
        LambdaInvocationDelegate next
    )
    {
        // This will clear the output formatting set by the Lambda runtime.
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

        await next(context);
    }
}
