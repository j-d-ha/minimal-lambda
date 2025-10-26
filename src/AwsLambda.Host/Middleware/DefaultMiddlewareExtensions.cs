namespace AwsLambda.Host;

public static class DefaultMiddlewareExtensions
{
    public static ILambdaApplication UseClearLambdaOutputFormatting(
        this ILambdaApplication application
    ) =>
        application.UseMiddleware(
            async (context, next) =>
            {
                // This will clear the output formatting set by the Lambda runtime.
                Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
                await next(context);
            }
        );
}
