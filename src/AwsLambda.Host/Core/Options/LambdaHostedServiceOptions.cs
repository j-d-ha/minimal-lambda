namespace AwsLambda.Host;

internal class LambdaHostedServiceOptions
{
    internal Action<ILambdaInvocationBuilder>? ConfigureHandlerBuilder { get; set; }
    internal Action<ILambdaOnInitBuilder>? ConfigureOnInitBuilder { get; set; }
    internal Action<ILambdaOnShutdownBuilder>? ConfigureOnShutdownBuilder { get; set; }
}
