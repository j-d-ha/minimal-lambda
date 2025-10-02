using Microsoft.CodeAnalysis;

namespace Lambda.Host.SourceGenerators;

internal static class Diagnostics
{
    internal static readonly DiagnosticDescriptor MultipleMethodCalls = new(
        "LH0001",
        "Multiple method calls detected",
        "Method '{0}' can only be invoked once per project. Remove this duplicate invocation.",
        "Usage",
        DiagnosticSeverity.Error,
        true
    );

    internal static readonly DiagnosticDescriptor MultipleParametersOfSameType = new(
        "LH0002",
        "Multiple parameters of the same type detected",
        "Handler method contains multiple parameters of type '{0}'. Only one parameter of this type is allowed.",
        "Usage",
        DiagnosticSeverity.Error,
        true
    );
}
