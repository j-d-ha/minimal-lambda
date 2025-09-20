using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Lambda.Host.SourceGenerators;

[Generator]
public class MapHandlerIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find all MapHandler method calls with lambda analysis
        var mapHandlerCalls = context
            .SyntaxProvider.CreateSyntaxProvider(
                static (node, token) => MapHandlerSyntaxProvider.Predicate(node, token),
                static (ctx, cancellationToken) =>
                    MapHandlerSyntaxProvider.Transformer(ctx, cancellationToken)
            )
            .Where(static m => m is not null)
            .Select(static (m, _) => m!);

        // Generate source when calls are found
        context.RegisterSourceOutput(
            mapHandlerCalls.Collect(),
            static (spc, calls) => GenerateLambdaReport(spc, calls)
        );
    }

    private static void GenerateLambdaReport(
        SourceProductionContext context,
        ImmutableArray<DelegateInfo> delegateInfos
    )
    {
        if (delegateInfos.Length == 0)
            return;

        var delegateInfo = delegateInfos.First();

        var delegateArguments = (delegateInfo.Parameters.Select(p => p.Type) ?? [])
            .Concat(
                new[] { delegateInfo?.ResponseType }.Where(t =>
                    t != null && t != TypeConstants.Void
                )
            )
            .ToList();

        var classFields = delegateInfo
            .Parameters.Where(p =>
                p.Attributes.All(a => a.Type != AttributeConstants.Request)
                && p.Type != TypeConstants.ILambdaContext
            )
            .Select(p => new
            {
                attributes = p.Attributes.Select(a => a.Type).ToList(),
                keyed_service_key = p
                    .Attributes.Where(a =>
                        a?.Type?.StartsWith(AttributeConstants.FromKeyedService) ?? false
                    )
                    .Select(a => a.Arguments.FirstOrDefault())
                    .FirstOrDefault(),
                name = p.ParameterName.ToCamelCase(),
                type = p.Type,
            })
            .ToList();

        var handlerArgs =
            delegateInfo.Parameters.Select(p => p.ParameterName.ToCamelCase()).ToList() ?? [];

        var lambdaParams =
            delegateInfo
                .Parameters.Where(p =>
                    p.Attributes.Any(a => a.Type == AttributeConstants.Request)
                    || p.Type == TypeConstants.ILambdaContext
                )
                .OrderBy(p => p.Type == TypeConstants.ILambdaContext ? 1 : 0)
                .Select(p => p.Type + " " + p.ParameterName.ToCamelCase())
                .ToList() ?? [];

        // 1. if Action -> no return
        // 3. if Func + Task return type + async -> no return
        // 2. if Func + Task return type -> return value
        // 4. if Func + non-Task return type -> return value
        var hasReturnValue = delegateInfo switch
        {
            { DelegateType: "Action" } => false,
            { DelegateType: "Func", IsAsync: true, ResponseType: TypeConstants.Task } => false,
            _ => true,
        };

        var model = new
        {
            @namespace = delegateInfo.Namespace,
            service = "LambdaStartupService",
            fields = classFields,
            delegate_type = delegateInfo.DelegateType,
            delegate_args = delegateArguments,
            handler_args = handlerArgs,
            lambda_params = lambdaParams,
            is_lambda_async = delegateInfo?.IsAsync ?? false,
            has_return_value = hasReturnValue,
        };

        var template = TemplateHelper.LoadTemplate(
            GeneratorConstants.LambdaStartupServiceTemplateFile
        );

        var outCode = template.Render(model);

        context.AddSource("LambdaStartup.g.cs", outCode);
    }
}

internal sealed class DelegateInfo
{
    internal required string? ResponseType { get; set; } = TypeConstants.Void;
    internal required string? Namespace { get; set; }
    internal required bool IsAsync { get; set; }

    internal string DelegateType => ResponseType == TypeConstants.Void ? "Action" : "Func";
    internal List<ParameterInfo> Parameters { get; set; } = [];
}

internal sealed class ParameterInfo
{
    internal required string? ParameterName { get; set; }
    internal required string? Type { get; set; }
    internal List<AttributeInfo> Attributes { get; set; } = [];
}

internal sealed class AttributeInfo
{
    internal required string? Type { get; set; }
    internal List<string> Arguments { get; set; } = [];
}
