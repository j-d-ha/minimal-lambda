namespace MinimalLambda;

public interface ILambdaMiddleware
{
    Task InvokeAsync(ILambdaInvocationContext context, LambdaInvocationDelegate next);
}
