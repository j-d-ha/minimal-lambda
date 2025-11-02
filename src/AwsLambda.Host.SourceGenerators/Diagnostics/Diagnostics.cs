using Microsoft.CodeAnalysis;

namespace AwsLambda.Host.SourceGenerators;

internal static class Diagnostics
{
    private const string UsageCategory = "AwsLambda.Host.Usage";

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

    internal static readonly DiagnosticDescriptor InvalidAttributeArgument = new(
        "LH0003",
        "Invalid attribute argument",
        "An argument of type '{0}' is not valid for this attribute. Please use a valid type.",
        UsageCategory,
        DiagnosticSeverity.Error,
        true
    );
}
