using System.Linq;
using MinimalLambda.SourceGenerators;
using MinimalLambda.SourceGenerators.Models;

namespace System.Collections.Generic;

internal static class EnumerableExtensions
{
    extension<T>(IEnumerable<T> enumerable)
    {
        public void ForEach(Action<T> action)
        {
            foreach (var item in enumerable)
                action(item);
        }

        internal (List<TOut> Data, List<DiagnosticInfo> Diagnostics) CollectDiagnosticResults<TOut>(
            Func<T, DiagnosticResult<TOut>> extractor) =>
            enumerable
                .Select(extractor)
                .Aggregate(
                    (Successes: new List<TOut>(enumerable is ICollection<T> c ? c.Count : 0),
                        Diagnostics: new List<DiagnosticInfo>()),
                    static (acc, result) =>
                    {
                        result.Switch(
                            info => acc.Successes.Add(info),
                            diagnostic => acc.Diagnostics.Add(diagnostic));

                        return acc;
                    });
    }
}
