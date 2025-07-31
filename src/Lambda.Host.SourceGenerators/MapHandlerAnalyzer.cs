using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Scriban;

namespace Lambda.Host.SourceGenerators;

[Generator]
public class MapHandlerLambdaAnalyzer : IIncrementalGenerator
{
    private const string LambdaContextType = "Amazon.Lambda.Core.ILambdaContext";
    private const string RequestAttribute = "Lambda.Host.RequestAttribute";
    private const string VoidType = "void";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find all MapHandler method calls with lambda analysis
        var mapHandlerCalls = context
            .SyntaxProvider.CreateSyntaxProvider(
                static (s, _) => IsMapHandlerCall(s),
                static (ctx, _) => AnalyzeMapHandlerLambda(ctx)
            )
            .Where(static m => m is not null);

        // Generate source when calls are found
        context.RegisterSourceOutput(
            mapHandlerCalls.Collect(),
            static (spc, calls) => GenerateLambdaReport(spc, calls)
        );
    }

    // Fast syntax check - look for MapHandler calls
    private static bool IsMapHandlerCall(SyntaxNode node)
    {
        if (node is not InvocationExpressionSyntax invocation)
            return false;

        return invocation.Expression is MemberAccessExpressionSyntax memberAccess
            && memberAccess.Name.Identifier.ValueText == "MapHandler";
    }

    // Analyze the lambda expression passed to MapHandler
    private static DelegateInfo? AnalyzeMapHandlerLambda(GeneratorSyntaxContext context)
    {
        if (context.Node is not InvocationExpressionSyntax invocationExpr)
            return null;

        if (invocationExpr.ArgumentList.Arguments.Count != 1)
            return null;

        var firstArgument = invocationExpr.ArgumentList.Arguments[0];

        // handle delegate expression
        if (
            firstArgument.Expression is IdentifierNameSyntax
            || firstArgument.Expression is MemberAccessExpressionSyntax
        )
            return ExtractInfoFromDelegate(context, firstArgument.Expression);

        // We can know that the lambda MUST be a ParenthesizedLambdaExpression as
        // SimpleLambdaExpression won't satisfy the Delegate type for MapHandler
        if (firstArgument.Expression is ParenthesizedLambdaExpressionSyntax lambda)
            return ExtractInfoFromLambda(context, lambda);

        return null;
    }

    private static string GetFileNamespace(SyntaxNode node, SemanticModel semanticModel)
    {
        // First try to find explicit namespace declaration
        var namespaceDeclaration = node.Ancestors()
            .OfType<BaseNamespaceDeclarationSyntax>()
            .FirstOrDefault();

        if (namespaceDeclaration != null)
        {
            return namespaceDeclaration.Name.ToString();
        }

        // For top-level statements, get the default namespace from compilation
        var compilation = semanticModel.Compilation;
        return compilation.Assembly.Name; // This will be "Lambda.Host.Example.HelloWorld"
    }

    private static DelegateInfo? ExtractInfoFromDelegate(
        GeneratorSyntaxContext context,
        ExpressionSyntax delegateExpression
    )
    {
        var semanticModel = context.SemanticModel;

        var symbolInfo = semanticModel.GetSymbolInfo(delegateExpression);

        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
        {
            // Handle method group case
            if (
                symbolInfo.CandidateSymbols.Length > 0
                && symbolInfo.CandidateSymbols[0] is IMethodSymbol candidateMethod
            )
                methodSymbol = candidateMethod;
            else
                return null;
        }

        // Extract method information
        var parameters = methodSymbol
            .Parameters.AsEnumerable()
            .Select(p => new ParameterInfo(
                p.Name,
                new TypeInfo(
                    p.Type.ToDisplayString(),
                    p.Type.ContainingNamespace.ToDisplayString()
                ),
                p.HasExplicitDefaultValue,
                p.HasExplicitDefaultValue,
                p.HasExplicitDefaultValue ? p.ExplicitDefaultValue?.ToString() : null,
                p.GetAttributes()
                    .Select(attr => new AttributeInfo(
                        attr.ToString(),
                        attr.AttributeClass?.ContainingNamespace.ToDisplayString()
                    ))
                    .ToList()
            ))
            .ToList();

        var responseType = new TypeInfo(
            methodSymbol.ReturnType.ToDisplayString(),
            methodSymbol.ReturnType.ContainingNamespace.ToDisplayString()
        );

        return new DelegateInfo(
            responseType,
            parameters,
            GetFileNamespace(context.Node, context.SemanticModel)
        );
    }

    private static DelegateInfo? ExtractInfoFromLambda(
        GeneratorSyntaxContext context,
        ParenthesizedLambdaExpressionSyntax lambdaExpression
    )
    {
        var lambdaTypeInfo = context.SemanticModel.GetTypeInfo(lambdaExpression);
        var delegateType = lambdaTypeInfo.ConvertedType as INamedTypeSymbol;

        // extract parameter information
        var parameters = lambdaExpression
            .ParameterList.Parameters.AsEnumerable()
            .Select(p => context.SemanticModel.GetDeclaredSymbol(p))
            .Where(p => p is not null)
            .Select(p =>
            {
                return new ParameterInfo(
                    p!.Name,
                    new TypeInfo(
                        p.Type.ToDisplayString(),
                        p.Type.ContainingNamespace.ToDisplayString()
                    ),
                    p.HasExplicitDefaultValue,
                    p.HasExplicitDefaultValue,
                    p.HasExplicitDefaultValue ? p.ExplicitDefaultValue?.ToString() : null,
                    p.GetAttributes()
                        .Select(attr => new AttributeInfo(
                            attr.ToString(),
                            attr.AttributeClass?.ContainingNamespace.ToDisplayString()
                        ))
                        .ToList()
                );
            })
            .ToList();

        // Hierarchy for determining lambda return type.
        //
        // 1. type conversion
        // 2. explicit return type
        // 3. implicit return type in expression body
        // 4. implicit return type in block body
        // 5. void

        var isAsync = lambdaExpression.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword);

        // Handle both expression and block bodies
        TypeInfo? returnTypeInfo = null;

        if (lambdaExpression.Body is ExpressionSyntax expression)
        {
            // Expression-bodied lambda: x => expression
            var typeInfo = context.SemanticModel.GetTypeInfo(expression);
            returnTypeInfo =
                typeInfo.Type != null
                    ? new TypeInfo(
                        typeInfo.Type.ToDisplayString(),
                        typeInfo.Type.ContainingNamespace.ToDisplayString()
                    )
                    : null;
        }
        else if (lambdaExpression.Body is BlockSyntax block)
        {
            // Block-bodied lambda: x => { statements; }
            // Find all return statements in the block
            var returnStatements = block
                .DescendantNodes()
                .OfType<ReturnStatementSyntax>()
                .Where(r => r.Expression != null)
                .ToList();

            if (returnStatements.Count > 0)
            {
                // Analyze the first return statement to determine the type
                var firstReturn = returnStatements.First();
                var typeInfo = context.SemanticModel.GetTypeInfo(firstReturn.Expression!);

                returnTypeInfo =
                    typeInfo.Type != null
                        ? new TypeInfo(
                            typeInfo.Type.ToDisplayString(),
                            typeInfo.Type.ContainingNamespace.ToDisplayString()
                        )
                        : null;
            }
            // If no return statements found, returnTypeInfo remains null (void)
        }

        var returnTypeName = (ReturnType: returnTypeInfo?.TypeName, IsAsync: isAsync) switch
        {
            (null, true) => "System.Threading.Tasks.Task",
            (null, false) => VoidType,
            (_, true) returnType => $"System.Threading.Tasks.Task<{returnType.ReturnType}>",
            var (typeName, _) => typeName,
        };

        var responseType = new TypeInfo(
            returnTypeName,
            returnTypeInfo?.Namespace ?? (isAsync ? "System.Threading.Tasks" : "System")
        );

        return new DelegateInfo(
            responseType,
            parameters,
            GetFileNamespace(context.Node, context.SemanticModel)
        );
    }

    private static void GenerateLambdaReport(
        SourceProductionContext context,
        ImmutableArray<DelegateInfo?> delegateInfos
    )
    {
        if (delegateInfos.Length == 0)
            return;

        var delegateInfo = delegateInfos.First();

        // TODO: build out lambda args parser
        // xTODO: add using imports for all types
        // xTODO: add constructor parameters for all injected types
        // xTODO: build out func type
        // xTODO: build out lambda handler arguments
        // xTODO: build out lambda invocation arguments
        // TODO: add guards around number of arguments with request attributes
        // TODO: look into handling duplicate field names
        // TODO: look into handling namespace vs type
        // TODO: add code to handle injecting ILambdaSerializer
        // TODO: add support for default values
        // TODO: validate that nullable types work as expected

        List<string> baseNamespaces =
        [
            "Amazon.Lambda.RuntimeSupport",
            "Amazon.Lambda.Serialization.SystemTextJson",
            "Microsoft.Extensions.DependencyInjection",
            "Microsoft.Extensions.Hosting",
        ];

        var template = Template.Parse(
            """
            // <auto-generated />
            #nullable enable

            {{~ for using_ in usings ~}}
            using {{ using_ }};
            {{~ end ~}}

            namespace {{ @namespace }};

            public class {{ service }} : IHostedService
            {
                private readonly DelegateHolder _delegateHolder;
                {{~ for field in fields ~}}
                private readonly {{ field.type }} _{{ field.name}};
                {{~ end ~}}

                public {{ service }}(DelegateHolder delegateHolder{{~ for field in fields ~}}, {{ for attribute in field.attributes }}[{{ attribute }}] {{ end }}{{ field.type }} {{ field.name }}{{~ end ~}})
                {
                    _delegateHolder = delegateHolder;
                    {{~ for field in fields ~}}
                    _{{ field.name }} = {{ field.name }};
                    {{~ end ~}}
                }

                public async Task StartAsync(CancellationToken cancellationToken)
                {
                    if (_delegateHolder.Handler is not {{ delegate_type }}<{{ delegate_args }}> lambdaHandler)
                        throw new InvalidOperationException("Invalid handler type.");

                    await LambdaBootstrapBuilder
                        .Create(
                            ({{ lambda_params | array.join ", " }}) => lambdaHandler({{ handler_args }}),
                            new DefaultLambdaJsonSerializer()
                        )
                        .Build()
                        .RunAsync(cancellationToken);
                }

                public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """
        );

        var delegateArguments = string.Join(
            ", ",
            (delegateInfo?.Parameters.Select(p => p.Type.TypeName) ?? new List<string>()).Concat(
                new[] { delegateInfo?.ResponseType.TypeName }.Where(t => t != null && t != VoidType)
            )
        );

        var classFields = delegateInfo
            ?.Parameters.Where(p =>
                p.Attributes.All(a => a.FullName != RequestAttribute)
                && p.Type.TypeName != LambdaContextType
            )
            .Select(p => new
            {
                attributes = p
                    .Attributes.Where(a => a.FullName is not null)
                    .Select(a => a.FullName)
                    .ToList(),
                name = p.ParameterName.ToCamelCase(),
                type = p.Type.TypeName,
            })
            .ToList();

        var lambdaArgs = string.Join(
            ", ",
            delegateInfo?.Parameters.Select(p => p.ParameterName.ToPrivateCamelCase()) ?? []
        );

        var lambdaParams =
            delegateInfo
                ?.Parameters.Where(p =>
                    p.Attributes.Any(a => a.FullName == RequestAttribute)
                    || p.Type.TypeName == LambdaContextType
                )
                .OrderBy(p => p.Type.TypeName == LambdaContextType ? 1 : 0)
                .Select(p => p.Type.TypeName + " " + p.ParameterName.ToPrivateCamelCase())
                .ToList() ?? [];

        var model = new
        {
            usings = baseNamespaces.Distinct().ToList(),
            @namespace = delegateInfo?.Namespace,
            service = "LambdaStartupService",
            fields = classFields,
            delegate_type = delegateInfo?.ResponseType.TypeName == VoidType ? "Action" : "Func",
            delegate_args = delegateArguments,
            handler_args = lambdaArgs,
            lambda_params = lambdaParams,
        };

        var outCode = template.Render(model);

        // context.AddSource("LambdaStartup.g.cs", outCode);
    }
}

public record DelegateInfo(
    TypeInfo ResponseType,
    List<ParameterInfo> Parameters,
    string? Namespace
);

public record ParameterInfo(
    string ParameterName,
    TypeInfo Type,
    bool IsOptional,
    bool HasDefaultValue,
    string? DefaultValue,
    List<AttributeInfo> Attributes
);

public record TypeInfo(string TypeName, string Namespace);

public record AttributeInfo(string? FullName, string? Namespace);
