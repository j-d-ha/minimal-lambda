namespace AwsLambda.Host;

internal interface ILambdaInvocationBuilderFactory
{
    ILambdaInvocationBuilder CreateBuilder();
}
