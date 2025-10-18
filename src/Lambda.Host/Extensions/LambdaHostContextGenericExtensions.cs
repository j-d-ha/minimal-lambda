namespace Lambda.Host;

public static class LambdaHostContextGenericExtensions
{
    public static T? GetResponse<T>(this ILambdaHostContext lambdaHostContext) =>
        lambdaHostContext.Response is T response ? response : default;

    public static T? GetRequest<T>(this ILambdaHostContext lambdaHostContext) =>
        lambdaHostContext.Request is T request ? request : default;
}
