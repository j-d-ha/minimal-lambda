namespace AwsLambda.Host.Builder;

internal interface ILambdaInvocationBuilderFactory
{
    ILambdaInvocationBuilder CreateBuilder();
}
