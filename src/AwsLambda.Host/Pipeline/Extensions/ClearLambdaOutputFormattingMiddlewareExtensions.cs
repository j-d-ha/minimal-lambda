namespace AwsLambda.Host;

/// <summary>
///     Provides extension methods for clearing Lambda runtime output formatting in the middleware
///     pipeline.
/// </summary>
public static class ClearLambdaOutputFormattingMiddlewareExtensions
{
    extension(ILambdaApplication application)
    {
        /// <summary>
        ///     Clears the output formatting applied by the Lambda runtime and resets console output to
        ///     standard behavior.
        /// </summary>
        /// <remarks>
        ///     The AWS Lambda runtime applies custom output formatting to the console stream. This
        ///     middleware resets the console output to use a standard <see cref="StreamWriter" /> with
        ///     auto-flushing enabled, allowing direct access to the standard output stream without Lambda's
        ///     formatting constraints. This is particularly useful when using structured logging frameworks
        ///     (such as Serilog with JSON formatting). The Lambda runtime's default formatting can interfere
        ///     with structured log output, potentially corrupting JSON payloads or breaking log parsing. By
        ///     clearing the output formatting, you ensure that your structured logs are written directly to
        ///     stdout without unwanted modifications.
        /// </remarks>
        /// <returns>The configured <see cref="ILambdaApplication" /> for method chaining.</returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <see cref="ILambdaApplication" /> is
        ///     <c>null</c>.
        /// </exception>
        public ILambdaApplication UseClearLambdaOutputFormatting()
        {
            ArgumentNullException.ThrowIfNull(application);

            application.Use(next =>
            {
                return async context =>
                {
                    // This will clear the output formatting set by the Lambda runtime.
                    Console.SetOut(
                        new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true }
                    );
                    await next(context);
                };
            });

            return application;
        }
    }
}
