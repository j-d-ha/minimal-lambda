using System.Linq;
using MinimalLambda.SourceGenerators.Models;

namespace Microsoft.CodeAnalysis;

internal static class IncrementalValueProviderExtensions
{
    extension<T>(IncrementalValuesProvider<T?> valueProviders) where T : class
    {
        public IncrementalValuesProvider<T> WhereNotNull() =>
            valueProviders.Where(static v => v is not null).Select(static (v, _) => v!);
    }

    extension<T>(IncrementalValuesProvider<T> valueProviders) where T : IMethodInfo
    {
        public IncrementalValuesProvider<T> WhereNoErrors() =>
            valueProviders.Where(static c => c.DiagnosticInfos.All(d =>
                d.DiagnosticDescriptor.DefaultSeverity != DiagnosticSeverity.Error));
    }
}
