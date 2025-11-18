namespace AwsLambda.Host;

public interface ILambdaOnShutdownBuilderFactory
{
    ILambdaOnShutdownBuilder CreateBuilder();
}
