using Microsoft.CodeAnalysis;

namespace MinimalLambda.SourceGenerators;

internal static class Diagnostics
{
    private const string UsageCategory = "AwsLambda.Host.Usage";
    private const string ConfigurationCategory = "AwsLambda.Host.Configuration";

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

    public static readonly DiagnosticDescriptor CSharpVersionTooLow = new(
        "LH0004",
        "C# language version too low",
        "AwsLambda.Host requires C# 11 or newer (or LanguageVersion=default with a modern SDK). "
            + "Set <LangVersion>latest</LangVersion> or enable preview features.",
        ConfigurationCategory,
        DiagnosticSeverity.Error,
        true
    );
}
