namespace Microsoft.CodeAnalysis;

internal static class IncrementalValueProviderExtensions
{
    extension<T>(IncrementalValuesProvider<T?> valueProviders)
        where T : class
    {
        public IncrementalValuesProvider<T> WhereNotNull() =>
            valueProviders.Where(static v => v is not null).Select(static (v, _) => v!);
    }
}
