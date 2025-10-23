using System;

namespace AwsLambda.Host.SourceGenerators.Extensions;

internal static class LinqExtensions
{
    internal static T Apply<T>(this T obj, Action<T> action)
        where T : class
    {
        action.Invoke(obj);
        return obj;
    }

    internal static Tout? Transform<Tin, Tout>(this Tin source, Func<Tin, Tout> transformer) =>
        transformer(source);
}
