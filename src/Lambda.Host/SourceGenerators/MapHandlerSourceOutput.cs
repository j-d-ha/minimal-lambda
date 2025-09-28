using System.Collections.Immutable;
using Lambda.Host.SourceGenerators.Extensions;
using Lambda.Host.SourceGenerators.Models;
using Microsoft.CodeAnalysis;

namespace Lambda.Host.SourceGenerators;

internal static class MapHandlerSourceOutput
{
    private static readonly DependencyInfo DelegateHolderInfo = new()
    {
        Type = "global::Lambda.Host.DelegateHolder",
        ParameterName = "delegateHolder",
    };

    private static readonly DependencyInfo ServiceProviderInfo = new()
    {
        Type = "global::System.IServiceProvider",
        ParameterName = "serviceProvider",
    };

    private static readonly DependencyInfo LambdaCancellationTokenSourceFactoryInfo = new()
    {
        Type = "global::Lambda.Host.Interfaces.ILambdaCancellationTokenSourceFactory",
        ParameterName = "lambdaCancellationTokenSourceFactory",
    };

    private static readonly ImmutableList<DependencyInfo> DefaultInjectedDependencies =
    [
        DelegateHolderInfo,
        ServiceProviderInfo,
    ];

    internal static void Generate(
        SourceProductionContext context,
        ImmutableArray<MapHandlerInvocationInfo> delegateInfos
    )
    {
        if (delegateInfos.Length == 0)
            return;

        var diagnostics = ValidateGeneratorData(delegateInfos);
        if (diagnostics.Any())
        {
            diagnostics.ForEach(context.ReportDiagnostic);
            return;
        }

        var delegateInfo = delegateInfos.First().DelegateInfo;

        // handle cancellation token
        //
        // ILambdaContext needs to be added to lambdaParams if a CancellationToken is requested and
        // the ILambdaContext is not asked for yet. If a ILambdaContext is asked for, and it is
        // named something other than lambdaContext, we need to assign it to the variable name
        // lambdaContext OR update out code to work with the new name.
        //
        // Will need to support multiple tokens asked for (stupid but easier than telling the user that).

        var isCancellationTokenRequested = delegateInfo.Parameters.Any(p =>
            p.Type == TypeConstants.CancellationToken
        );

        var injectedDependencies = DefaultInjectedDependencies
            .ToList()
            .Concat(isCancellationTokenRequested ? [LambdaCancellationTokenSourceFactoryInfo] : [])
            .Select(di => new
            {
                type = di.Type,
                parameter_name = di.ParameterName,
                field_name = di.FieldName,
            })
            .ToList();

        var delegateArguments = delegateInfo
            .Parameters.Select(p => p.Type)
            .Concat(
                new[] { delegateInfo.ResponseType }.Where(t => t != null && t != TypeConstants.Void)
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

        var handlerArgs = delegateInfo
            .Parameters.Select(p => p.ParameterName.ToCamelCase())
            .ToList();

        var lambdaParams = delegateInfo
            .Parameters.Where(p =>
                p.Attributes.Any(a => a.Type == AttributeConstants.Request)
                || p.Type == TypeConstants.ILambdaContext
            )
            .OrderBy(p => p.Type == TypeConstants.ILambdaContext ? 1 : 0)
            .Select(p => p.Type + " " + p.ParameterName.ToCamelCase())
            .ToList();

        // 1. if Action -> no return
        // 3. if Func + Task return type + async -> no return
        // 2. if Func + Task return type -> return value
        // 4. if Func + non-Task return type -> return value
        var hasReturnValue = delegateInfo switch
        {
            { DelegateType: TypeConstants.Action } => false,
            { DelegateType: TypeConstants.Func, IsAsync: true, ResponseType: TypeConstants.Task } =>
                false,
            _ => true,
        };

        var model = new
        {
            @namespace = delegateInfo.Namespace,
            service = "LambdaStartupService",
            injected_dependencies = injectedDependencies,
            fields = classFields,
            delegate_type = delegateInfo.DelegateType,
            delegate_args = delegateArguments,
            handler_args = handlerArgs,
            lambda_params = lambdaParams,
            is_lambda_async = delegateInfo.IsAsync,
            has_return_value = hasReturnValue,
        };

        var template = TemplateHelper.LoadTemplate(
            GeneratorConstants.LambdaStartupServiceTemplateFile
        );

        var outCode = template.Render(model);

        context.AddSource("LambdaStartup.g.cs", outCode);
    }

    private static List<Diagnostic> ValidateGeneratorData(
        ImmutableArray<MapHandlerInvocationInfo> delegateInfos
    )
    {
        var diagnostics = new List<Diagnostic>();

        // check for multiple invocations of MapHandler
        diagnostics.AddRange(
            delegateInfos
                .Skip(1)
                .Select(invocationInfo =>
                    Diagnostic.Create(
                        Diagnostics.MultipleMethodCalls,
                        invocationInfo.Location,
                        "LambdaApplication.MapHandler(Delegate)"
                    )
                )
        );

        return diagnostics;
    }
}
