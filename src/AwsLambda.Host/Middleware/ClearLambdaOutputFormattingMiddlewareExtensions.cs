namespace AwsLambda.Host;

public static class ClearLambdaOutputFormattingMiddlewareExtensions
{
    /// <summary>
    /// Clears the output formatting applied by the Lambda runtime and resets console output to standard behavior.
    /// </summary>
    /// <remarks>
    /// The AWS Lambda runtime applies custom output formatting to the console stream. This middleware
    /// resets the console output to use a standard <see cref="StreamWriter"/> with auto-flushing enabled,
    /// allowing direct access to the standard output stream without Lambda's formatting constraints.
    ///
    /// This is particularly useful when using structured logging frameworks (such as Serilog with JSON formatting).
    /// The Lambda runtime's default formatting can interfere with structured log output, potentially corrupting
    /// JSON payloads or breaking log parsing. By clearing the output formatting, you ensure that your structured
    /// logs are written directly to stdout without unwanted modifications.
    /// </remarks>
    /// <param name="application">The Lambda application to configure.</param>
    /// <returns>The configured <see cref="ILambdaApplication"/> for method chaining.</returns>
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
