using System;
using System.Collections.Generic;
using System.Linq;

namespace AwsLambda.Host.SourceGenerators.Types;

internal static class EquatableArrayExtensions
{
    internal static EquatableArray<T> ToEquatableArray<T>(this IEnumerable<T> enumerable)
        where T : IEquatable<T> => new(enumerable.ToArray());
}
