namespace AwsLambda.Host;

public static class LambdaHostContextExtensions
{
    public static T? GetEvent<T>(this ILambdaHostContext context)
    {
        if (context.Event is T eventT)
            return eventT;

        return default;
    }

    public static bool TryGetEvent<T>(this ILambdaHostContext context, out T? result)
    {
        if (context.Event is T eventT)
        {
            result = eventT;
            return true;
        }

        result = default;
        return false;
    }

    public static T? GetResponse<T>(this ILambdaHostContext context)
    {
        if (context.Response is T responseT)
            return responseT;

        var x = default(T?);

        return x;
    }

    public static bool TryGetResponse<T>(this ILambdaHostContext context, out T? result)
    {
        if (context.Response is T responseT)
        {
            result = responseT;
            return true;
        }

        result = default;
        return false;
    }
}
