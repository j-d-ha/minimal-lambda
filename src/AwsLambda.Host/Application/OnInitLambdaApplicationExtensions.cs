namespace AwsLambda.Host;

public static class OnInitLambdaApplicationExtensions
{
    public static ILambdaApplication OnInitClearLambdaOutputFormatting(
        this ILambdaApplication application
    )
    {
        ArgumentNullException.ThrowIfNull(application);

        application.OnInit(
            Task<bool> (_, _) =>
            {
                // This will clear the output formatting set by the Lambda runtime.
                Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

                return Task.FromResult(true);
            }
        );

        return application;
    }
}
