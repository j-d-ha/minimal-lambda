using System;
using System.Collections.Generic;
using System.Linq;

namespace Lambda.Host.SourceGenerators.Types;

internal static class EquatableArrayLinqExtensions
{
    internal static EquatableArray<T> ToEquatableArray<T>(this IEnumerable<T> enumerable)
        where T : IEquatable<T> => new(enumerable.ToArray());
}
