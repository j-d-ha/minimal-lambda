using System.Collections.Immutable;

namespace Lambda.Host.SourceGenerators.Models;

internal readonly record struct AttributeInfo(string Type, ImmutableArray<string> Arguments);
