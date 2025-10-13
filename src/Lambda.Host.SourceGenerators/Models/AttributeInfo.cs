using Lambda.Host.SourceGenerators.Types;

namespace Lambda.Host.SourceGenerators.Models;

internal readonly record struct AttributeInfo(string Type, EquatableArray<string> Arguments);
