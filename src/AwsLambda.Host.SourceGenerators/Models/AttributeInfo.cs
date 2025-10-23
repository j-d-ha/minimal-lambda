using AwsLambda.Host.SourceGenerators.Types;

namespace AwsLambda.Host.SourceGenerators.Models;

internal readonly record struct AttributeInfo(string Type, EquatableArray<string> Arguments);
