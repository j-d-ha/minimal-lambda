using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AwsLambda.Host;

public static class OutputFormattingLambdaApplicationExtensions
{
    /// <summary>Clears Lambda runtime output formatting and resets console output to standard behavior.</summary>
    /// <remarks>
    ///     The AWS Lambda runtime applies custom formatting to console output. This method resets the
    ///     console to use a standard <see cref="StreamWriter" /> with auto-flushing, which is essential
    ///     for structured logging frameworks like Serilog to output JSON without corruption.
    /// </remarks>
    /// <param name="application">The Lambda application to configure.</param>
    /// <returns>The configured <see cref="ILambdaApplication" /> for method chaining.</returns>
    public static ILambdaApplication OnInitClearLambdaOutputFormatting(
        this ILambdaApplication application
    )
    {
        ArgumentNullException.ThrowIfNull(application);

        application.OnInit(
            Task<bool> (services, _) =>
            {
                var logger = services.GetService<ILogger<LambdaHostedService>>();

                // This will clear the output formatting set by the Lambda runtime.
                Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

                logger?.LogInformation("Clearing Lambda output formatting");

                return Task.FromResult(true);
            }
        );

        return application;
    }
}
