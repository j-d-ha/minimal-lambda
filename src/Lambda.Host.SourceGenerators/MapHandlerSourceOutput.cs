using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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

    private static readonly DependencyInfo ILambdaSerializerInfo = new()
    {
        Type = "global::Amazon.Lambda.Core.ILambdaSerializer",
        ParameterName = "lambdaSerializer",
    };

    private static readonly DependencyInfo ILambdaContextInfo = new()
    {
        Type = TypeConstants.ILambdaContext,
        ParameterName = TypeConstants.ILambdaContextName,
    };

    private static readonly ImmutableList<DependencyInfo> DefaultInjectedDependencies =
    [
        DelegateHolderInfo,
        ServiceProviderInfo,
    ];

    internal static void Generate(SourceProductionContext context, CompilationInfo compilationInfo)
    {
        // validate the generator data and report any diagnostics before exiting.
        var diagnostics = ValidateGeneratorData(compilationInfo);
        if (diagnostics.Any())
        {
            diagnostics.ForEach(context.ReportDiagnostic);
            return;
        }

        // if no MapHandler calls were found, we will silently exit early.
        if (compilationInfo.MapHandlerInvocationInfos.Length == 0)
            return;

        var delegateInfo = compilationInfo.MapHandlerInvocationInfos.First().DelegateInfo;
        StartupClassInfo? startupClassInfo = compilationInfo.StartupClassInfos.FirstOrDefault();

        // Report generation mode to the user
        context.ReportDiagnostic(
            Diagnostic.Create(
                Diagnostics.GenerationMode,
                null,
                startupClassInfo is not null
                    ? $"Generating partial class implementation for '{startupClassInfo?.ClassName}'"
                    : $"Generating standalone '{GeneratorConstants.StartupClassName}' class"
            )
        );

        var isSerializerNeeded = IsSerializerNeeded(delegateInfo);

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
            .Concat(isSerializerNeeded ? [ILambdaSerializerInfo] : [])
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
                p.Attributes.All(a => a.Type != AttributeConstants.RequestAttribute)
                && p.Type != TypeConstants.ILambdaContext
                && p.Type != TypeConstants.CancellationToken
            )
            .Select(p => new
            {
                attributes = p.Attributes.Select(a => a.Type).ToList(),
                keyed_service_key = p
                    .Attributes.Where(a => a.Type.StartsWith(AttributeConstants.FromKeyedService))
                    .Select(a => a.Arguments.FirstOrDefault())
                    .FirstOrDefault(),
                name = p.ParameterName.ToCamelCase(),
                type = p.Type,
            })
            .ToList();

        var handlerArgs = delegateInfo
            .Parameters.Select(p => new { name = p.ParameterName.ToCamelCase(), type = p.Type })
            .ToList();

        var lambdaParams = delegateInfo
            .Parameters.Concat(
                isCancellationTokenRequested
                && delegateInfo.Parameters.All(p => p.Type != TypeConstants.ILambdaContext)
                    ?
                    [
                        new ParameterInfo
                        {
                            ParameterName = ILambdaContextInfo.InternalVariableName,
                            LocationInfo = null,
                            Type = ILambdaContextInfo.Type,
                        },
                    ]
                    : []
            )
            .Where(p =>
                (
                    !p.Attributes.IsDefaultOrEmpty
                    && p.Attributes.Any(a => a.Type == AttributeConstants.RequestAttribute)
                )
                || p.Type == TypeConstants.ILambdaContext
            )
            .OrderBy(p => p.Type == TypeConstants.ILambdaContext ? 1 : 0)
            .Select(p => new { type = p.Type, name = p.ParameterName.ToCamelCase() })
            .ToList();

        var cancellationTokenDetails = new
        {
            is_cancellation_token_requested = isCancellationTokenRequested,
            lambda_context_parameter_name = lambdaParams
                .FirstOrDefault(cf => cf.type == TypeConstants.ILambdaContext)
                ?.name,
            cancellation_token_var_name = handlerArgs
                .Where(ha => ha.type == TypeConstants.CancellationToken)
                .Select(ha => ha.name)
                .FirstOrDefault(),
        };

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
            IsPartialClass = startupClassInfo is not null,
            Accessibility = startupClassInfo?.Accessibility ?? "public",
            Service = startupClassInfo?.ClassName ?? GeneratorConstants.StartupClassName,
            InjectedDependencies = injectedDependencies,
            ClassFields = classFields,
            delegateInfo.DelegateType,
            DelegateArgs = delegateArguments,
            HandlerArgs = handlerArgs,
            LambdaParams = lambdaParams,
            IsLambdaAsync = delegateInfo.IsAsync,
            HasReturnValue = hasReturnValue,
            CancellationTokenDetails = cancellationTokenDetails,
            IsSerializerNeeded = isSerializerNeeded,
            IsGlobalNamespace = startupClassInfo?.Namespace == GeneratorConstants.GlobalNamespace,
            ClassNamespace = startupClassInfo?.Namespace ?? delegateInfo.Namespace,
        };

        var template = TemplateHelper.LoadTemplate(
            GeneratorConstants.LambdaStartupServiceTemplateFile
        );

        var outCode = template.Render(model);

        context.AddSource("LambdaStartup.g.cs", outCode);
    }

    private static List<Diagnostic> ValidateGeneratorData(CompilationInfo compilationInfo)
    {
        var diagnostics = new List<Diagnostic>();

        var delegateInfos = compilationInfo.MapHandlerInvocationInfos;
        var startupClassInfos = compilationInfo.StartupClassInfos;

        // check for multiple invocations of MapHandler
        if (delegateInfos.Length > 1)
            diagnostics.AddRange(
                delegateInfos.Select(invocationInfo =>
                    Diagnostic.Create(
                        Diagnostics.MultipleMethodCalls,
                        invocationInfo.LocationInfo?.ToLocation(),
                        "LambdaApplication.MapHandler(Delegate)"
                    )
                )
            );

        // check for multiple classes decorated with LambdaStartup
        if (startupClassInfos.Length > 1)
            diagnostics.AddRange(
                startupClassInfos.Select(startupClassInfo =>
                    Diagnostic.Create(
                        Diagnostics.MultipleClassesWithAttribute,
                        startupClassInfo?.LocationInfo?.ToLocation(),
                        "LambdaHostAttribute"
                    )
                )
            );

        // Validate parameters
        foreach (var invocationInfo in delegateInfos)
        {
            // check for more than one ILambdaContext parameter or CancellationToken parameter
            CheckForDuplicateTypeParameters(
                invocationInfo.DelegateInfo.Parameters,
                TypeConstants.CancellationToken
            );

            // check for more than one ILambdaContext parameter or CancellationToken parameter
            CheckForDuplicateTypeParameters(
                invocationInfo.DelegateInfo.Parameters,
                TypeConstants.ILambdaContext
            );

            // check for any parameter names using the reserved prefix `__`
            const string reservedPrefix = "__";
            diagnostics.AddRange(
                invocationInfo
                    .DelegateInfo.Parameters.Where(p => p.ParameterName.StartsWith(reservedPrefix))
                    .Select(p =>
                        Diagnostic.Create(
                            Diagnostics.ParameterUsesReservedPrefix,
                            p.LocationInfo?.ToLocation(),
                            p.ParameterName,
                            reservedPrefix
                        )
                    )
            );

            // check for multiple parameters that use the `[Request]` attribute
            if (
                invocationInfo.DelegateInfo.Parameters.Count(p =>
                    p.Attributes.Any(a => a.Type == AttributeConstants.RequestAttribute)
                ) > 1
            )
                diagnostics.AddRange(
                    invocationInfo
                        .DelegateInfo.Parameters.Where(p =>
                            p.Attributes.Any(a => a.Type == AttributeConstants.RequestAttribute)
                        )
                        .Select(p =>
                            Diagnostic.Create(
                                Diagnostics.MultipleParametersUseAttribute,
                                p.LocationInfo?.ToLocation(),
                                AttributeConstants.RequestAttribute
                            )
                        )
                );
        }

        return diagnostics;

        void CheckForDuplicateTypeParameters(
            ImmutableArray<ParameterInfo> parameterInfos,
            string type
        )
        {
            if (parameterInfos.Count(p => p.Type == type) > 1)
                diagnostics.AddRange(
                    parameterInfos
                        .Where(p => p.Type == type)
                        .Select(p =>
                            Diagnostic.Create(
                                Diagnostics.MultipleParametersOfSameType,
                                p.LocationInfo?.ToLocation(),
                                type
                            )
                        )
                );
        }
    }

    /// <summary>
    ///     Determines if a serializer is required for the provided delegate information.
    /// </summary>
    /// <remarks>
    ///     A Lambda handler needs a serializer when it uses custom .NET types for input or output
    ///     that require conversion between Lambda's JSON format and .NET objects.
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 Has a custom input parameter (not <c>Stream</c> or <c>ILambdaContext</c>)
    ///                 requiring JSON deserialization
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 Returns a custom type (not <c>Stream</c>, <c>void</c>, or <c>Task</c>)
    ///                 requiring JSON serialization
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    /// <param name="delegateInfo">The information about the delegate being analyzed.</param>
    /// <returns>True if a serializer is needed; otherwise, false.</returns>
    private static bool IsSerializerNeeded(DelegateInfo delegateInfo)
    {
        // true if the handler has a custom input parameter requiring JSON deserialization
        var inputType = delegateInfo
            .Parameters.FirstOrDefault(p =>
                p.Attributes.Any(a => a.Type == AttributeConstants.RequestAttribute)
            )
            .Type;

        if (inputType is not null and not TypeConstants.Stream)
            return true;

        // true if the handler returns a type not Stream, void, or Task
        return delegateInfo.ResponseType
            is not TypeConstants.Stream
                and not TypeConstants.Void
                and not TypeConstants.Task;
    }
}
