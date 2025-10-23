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

    internal static readonly DiagnosticDescriptor MultipleParametersUseAttribute = new(
        "LH0002",
        "Multiple parameters use attribute",
        "Handler method contains multiple parameters that use the '{0}' attribute. Only one parameter can use this attribute.",
        UsageCategory,
        DiagnosticSeverity.Error,
        true
    );

    internal static readonly DiagnosticDescriptor GenerationMode = new(
        "LH1001",
        "Source generation mode",
        "{0}",
        UsageCategory,
        DiagnosticSeverity.Info,
        true
    );
}
