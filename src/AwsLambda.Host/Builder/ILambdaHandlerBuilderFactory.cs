namespace AwsLambda.Host;

internal interface ILambdaHandlerBuilderFactory
{
    ILambdaInvocationBuilder CreateBuilder();
}
