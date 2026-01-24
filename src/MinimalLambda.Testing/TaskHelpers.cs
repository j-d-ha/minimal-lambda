namespace MinimalLambda.Testing;

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
            .Where(t => t is { Exception: not null }
                or Task<Exception?> { Status: TaskStatus.RanToCompletion, Result: not null }
                or Task<object?> { Status: TaskStatus.RanToCompletion, Result: Exception })
            .Select(t => t switch
            {
                { Exception: not null } => t.Exception!,
                Task<Exception?> { Result: { } ex } => ex,
                Task<object?> { Result: Exception ex } => ex,
                _ => null,
            })
            .Where(static ex => ex is not null)
            .Cast<Exception>()
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
