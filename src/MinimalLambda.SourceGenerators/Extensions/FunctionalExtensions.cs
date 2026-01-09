namespace System;

internal static class FunctionalExtensions
{
    extension<T>(T source)
    {
        public TResult Map<TResult>(Func<T, TResult> func) => func(source);
    }
}
