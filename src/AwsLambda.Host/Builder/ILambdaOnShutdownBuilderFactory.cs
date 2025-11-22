namespace AwsLambda.Host.Builder;

internal interface ILambdaOnShutdownBuilderFactory
{
    ILambdaOnShutdownBuilder CreateBuilder();
}
