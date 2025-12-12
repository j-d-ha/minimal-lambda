namespace MinimalLambda.Testing;

internal static class DictionaryExtensions
{
    extension<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
    {
        internal void AddRequired(TKey key, TValue value)
        {
            if (!dictionary.TryAdd(key, value))
                throw new InvalidOperationException($"Key '{key}' already exists.");
        }

        internal void GetRequired(TKey? key, out TValue value)
        {
            if (key is null || !dictionary.TryGetValue(key, out value!))
                throw new InvalidOperationException($"Key '{key}' is null or does not exist.");
        }
    }
}
