using Microsoft.CodeAnalysis;

namespace Lambda.Host.SourceGenerators;

internal static class Diagnostics
{
    private const string UsageCategory = "Lambda.Host.Usage";

    internal static readonly DiagnosticDescriptor MultipleMethodCalls = new(
        "LH0001",
        "Multiple method calls detected",
        "Method '{0}' can only be invoked once per project. Remove this duplicate invocation.",
        UsageCategory,
        DiagnosticSeverity.Error,
        true
    );

    internal static readonly DiagnosticDescriptor MultipleParametersOfSameType = new(
        "LH0002",
        "Multiple parameters of the same type detected",
        "Handler method contains multiple parameters of type '{0}'. Only one parameter of this type is allowed.",
        UsageCategory,
        DiagnosticSeverity.Error,
        true
    );

    internal static readonly DiagnosticDescriptor ParameterUsesReservedPrefix = new(
        "LH0003",
        "Parameter name uses reserved prefix",
        "Parameter name '{0}' uses reserved prefix '{1}'. Remove the prefix from this parameter name.",
        UsageCategory,
        DiagnosticSeverity.Error,
        true
    );
}
