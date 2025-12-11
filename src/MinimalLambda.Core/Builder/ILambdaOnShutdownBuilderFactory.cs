namespace MinimalLambda.Host.Builder;

internal interface ILambdaOnShutdownBuilderFactory
{
    ILambdaOnShutdownBuilder CreateBuilder();
}
