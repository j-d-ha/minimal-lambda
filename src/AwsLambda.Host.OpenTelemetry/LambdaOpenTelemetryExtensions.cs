namespace AwsLambda.Host;

public static class LambdaOpenTelemetryExtensions
{
    public static ILambdaApplication UseOpenTelemetryTracing(this ILambdaApplication application)
    {
        return application;
    }
}
