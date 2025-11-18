namespace AwsLambda.Host;

public interface ILambdaHandlerBuilderFactory
{
    ILambdaHandlerBuilder CreateBuilder();
}
