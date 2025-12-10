namespace AwsLambda.Host.Testing;

internal static class TaskHelpers
{
    internal static async Task<Exception[]> WhenAny(params Task[] tasks)
    {
        await Task.WhenAny(tasks);
        return ExtractExceptions(tasks);
    }

    internal static async Task<Exception[]> WhenAll(params Task[] tasks)
    {
        await Task.WhenAll(tasks);
        return ExtractExceptions(tasks);
    }

    private static Exception[] ExtractExceptions(Task[] tasks) =>
        tasks
            .Where(t => t is { IsFaulted: true, Exception: not null })
            .Select(e =>
                e.Exception!.InnerExceptions.Count > 1
                    ? e.Exception
                    : e.Exception.InnerExceptions[0]
            )
            .ToArray();

    extension(Task<Exception[]> exceptionsTask)
    {
        internal async Task UnwrapAndThrow(string errorMessage)
        {
            var exceptions = await exceptionsTask;
            if (exceptions.Length > 0)
                throw new AggregateException(errorMessage, exceptions);
        }
    }
}
