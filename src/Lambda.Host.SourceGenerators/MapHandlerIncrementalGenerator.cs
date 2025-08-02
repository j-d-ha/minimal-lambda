using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lambda.Host.SourceGenerators;

[Generator]
public class MapHandlerIncrementalGenerator : IIncrementalGenerator
{
    private const string LambdaContextType = "global::Amazon.Lambda.Core.ILambdaContext";
    private const string RequestAttribute = "global::Lambda.Host.RequestAttribute";
    private const string VoidType = "void";
    private const string TaskType = "global::System.Threading.Tasks.Task";

    private const string KeyedServiceAttribute =
        "Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute";

    private const string LambdaApplicationClassName = "LambdaApplication";
    private const string MapHandlerMethodName = "MapHandler";

    private const string LambdaStartupServiceTemplateFile =
        "Templates/LambdaStartupService.scriban";

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

        return invocation.Expression
            is MemberAccessExpressionSyntax { Name.Identifier.ValueText: MapHandlerMethodName };
    }

    // Analyze the lambda expression passed to MapHandler
    private static DelegateInfo? AnalyzeMapHandlerLambda(GeneratorSyntaxContext context)
    {
        // validate that the method is from the LambdaApplication type
        if (context.Node is not InvocationExpressionSyntax invocationExpr)
            return null;

        // Get the method symbol being invoked
        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocationExpr);

        // var methodSymbol = symbolInfo.Symbol as IMethodSymbol;
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
        {
            if (symbolInfo.CandidateSymbols.Length == 0)
                return null;

            // If direct resolution fails, check candidates
            var candidates = symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().ToList();
            if (candidates.Count == 0)
                return null;

            // Pick the first candidate or apply your own logic to choose
            methodSymbol = candidates.First();
        }

        // Check if it's from LambdaApplication
        if (methodSymbol?.ContainingType?.Name != LambdaApplicationClassName)
            return null;

        // // commented out as we need to be able to handle type casts which will have two arguments
        // if (invocationExpr.ArgumentList.Arguments.Count != 1)
        //     return null;

        var firstArgument = invocationExpr.ArgumentList.Arguments[0];

        return firstArgument.Expression switch
        {
            // handle delegate expression
            IdentifierNameSyntax or MemberAccessExpressionSyntax => ExtractInfoFromDelegate(
                context,
                firstArgument.Expression
            ),

            // We can know that the lambda MUST be a ParenthesizedLambdaExpression as
            // SimpleLambdaExpression won't satisfy the Delegate type for MapHandler
            ParenthesizedLambdaExpressionSyntax lambda => ExtractInfoFromLambda(context, lambda),

            // check for cast expression
            CastExpressionSyntax castExpression => ExtractInfoFromCastLambda(
                context,
                castExpression
            ),

            _ => null,
        };
    }

    private static string GetFileNamespace(SyntaxNode node, SemanticModel semanticModel)
    {
        // First try to find explicit namespace declaration
        var namespaceDeclaration = node.Ancestors()
            .OfType<BaseNamespaceDeclarationSyntax>()
            .FirstOrDefault();

        if (namespaceDeclaration != null)
            return namespaceDeclaration.Name.ToString();

        // For top-level statements, get the default namespace from compilation
        var compilation = semanticModel.Compilation;
        return compilation.Assembly.Name; // This will be "Lambda.Host.Example.HelloWorld"
    }

    private static DelegateInfo? ExtractInfoFromDelegate(
        GeneratorSyntaxContext context,
        ExpressionSyntax delegateExpression
    )
    {
        var symbolInfo = context.SemanticModel.GetSymbolInfo(delegateExpression);

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

        var parameters = methodSymbol
            .Parameters.AsEnumerable()
            .Select(p =>
            {
                return new ParameterInfo
                {
                    ParameterName = p!.Name,
                    Type = p.Type.GetAsGlobal(),
                    Attributes = p.GetAttributes()
                        .Select(a => new AttributeInfo
                        {
                            Type = a.ToString(),
                            Arguments = a
                                .ConstructorArguments.Select(aa => aa.Value?.ToString())
                                .Where(aa => aa is not null)
                                .ToList()!,
                        })
                        .ToList(),
                };
            })
            .ToList();

        return new DelegateInfo
        {
            ResponseType = methodSymbol.ReturnType.GetAsGlobal(),
            Namespace = GetFileNamespace(context.Node, context.SemanticModel),
            Parameters = parameters,
        };
    }

    private static DelegateInfo? ExtractInfoFromCastLambda(
        GeneratorSyntaxContext context,
        CastExpressionSyntax castExpression
    )
    {
        var castType = context.SemanticModel.GetTypeInfo(castExpression.Type).Type;

        if (castType is not INamedTypeSymbol namedType)
            return null;

        var invokeMethod = namedType.GetMembers("Invoke").OfType<IMethodSymbol>().FirstOrDefault();

        // The cast expression contains a parenthesized expression, which contains the lambda
        if (castExpression.Expression is not ParenthesizedExpressionSyntax parenthesizedExpr)
            return null;

        // Now get the lambda from inside the parentheses
        if (parenthesizedExpr.Expression is not ParenthesizedLambdaExpressionSyntax innerLambda)
            return null;

        var symbolInfo = context.SemanticModel.GetSymbolInfo(innerLambda);

        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol2)
        {
            // Handle method group case
            if (
                symbolInfo.CandidateSymbols.Length > 0
                && symbolInfo.CandidateSymbols[0] is IMethodSymbol candidateMethod
            )
                methodSymbol2 = candidateMethod;
            else
                return null;
        }

        var parameters =
            invokeMethod
                ?.Parameters.Zip<IParameterSymbol?, IParameterSymbol, ParameterInfo?>(
                    methodSymbol2.Parameters,
                    (t, p) =>
                        t is not null && p is not null
                            ? new ParameterInfo
                            {
                                ParameterName = p.Name,
                                Type = t.Type.GetAsGlobal(),
                                Attributes = p.GetAttributes()
                                    .Select(a => new AttributeInfo
                                    {
                                        Type = a.ToString(),
                                        Arguments = a
                                            .ConstructorArguments.Select(aa => aa.Value?.ToString())
                                            .Where(aa => aa is not null)
                                            .ToList()!,
                                    })
                                    .ToList(),
                            }
                            : null
                )
                .Where(p => p is not null)
                .ToList() ?? [];

        return new DelegateInfo
        {
            ResponseType = invokeMethod?.ReturnType.GetAsGlobal() ?? VoidType,
            Namespace = GetFileNamespace(context.Node, context.SemanticModel),
            Parameters = parameters!,
        };
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
                return new ParameterInfo
                {
                    ParameterName = p!.Name,
                    Type = p.Type.GetAsGlobal(),
                    Attributes = p.GetAttributes()
                        .Select(a => new AttributeInfo
                        {
                            Type = a.ToString(),
                            Arguments = a
                                .ConstructorArguments.Select(aa => aa.Value?.ToString())
                                .Where(aa => aa is not null)
                                .ToList()!,
                        })
                        .ToList(),
                };
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
        string? returnType = null;

        if (lambdaExpression.Body is ExpressionSyntax expression)
        {
            // Expression-bodied lambda: x => expression
            var typeInfo = context.SemanticModel.GetTypeInfo(expression);
            returnType = typeInfo.Type?.GetAsGlobal();
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

                returnType = typeInfo.Type?.GetAsGlobal();
            }
            // If no return statements found, returnTypeInfo remains null (void)
        }

        var returnTypeName = (ReturnType: returnType, IsAsync: isAsync) switch
        {
            (null, true) => TaskType,
            (null, false) => VoidType,
            (var type, true) => $"{TaskType}<{type}>",
            var (type, _) => type,
        };

        return new DelegateInfo
        {
            ResponseType = returnTypeName,
            Namespace = GetFileNamespace(context.Node, context.SemanticModel),
            Parameters = parameters,
        };
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
        // TODO: handle default values for parameters
        // TODO: add guards around number of arguments with request attributes
        // TODO: look into handling duplicate field names
        // TODO: look into handling namespace vs type
        // TODO: add code to handle injecting ILambdaSerializer
        // TODO: add support for default values
        // TODO: validate that nullable types work as expected
        // TODO: update to handle situations where serializer is not needed

        var delegateArguments = (delegateInfo?.Parameters.Select(p => p.Type) ?? [])
            .Concat(new[] { delegateInfo?.ResponseType }.Where(t => t != null && t != VoidType))
            .ToList();

        var classFields = delegateInfo
            ?.Parameters.Where(p =>
                p.Attributes.All(a => a.Type != RequestAttribute) && p.Type != LambdaContextType
            )
            .Select(p => new
            {
                attributes = p.Attributes.Select(a => a.Type).ToList(),
                keyed_service_key = p
                    .Attributes.Where(a => a?.Type?.StartsWith(KeyedServiceAttribute) ?? false)
                    .Select(a => a.Arguments.FirstOrDefault())
                    .FirstOrDefault(),
                name = p.ParameterName.ToCamelCase(),
                type = p.Type,
            })
            .ToList();

        var handlerArgs =
            delegateInfo?.Parameters.Select(p => p.ParameterName.ToCamelCase()).ToList() ?? [];

        var lambdaParams =
            delegateInfo
                ?.Parameters.Where(p =>
                    p.Attributes.Any(a => a.Type == RequestAttribute) || p.Type == LambdaContextType
                )
                .OrderBy(p => p.Type == LambdaContextType ? 1 : 0)
                .Select(p => p.Type + " " + p.ParameterName.ToCamelCase())
                .ToList() ?? [];

        var model = new
        {
            @namespace = delegateInfo?.Namespace,
            service = "LambdaStartupService",
            fields = classFields,
            delegate_type = delegateInfo?.ResponseType == VoidType ? "Action" : "Func",
            delegate_args = delegateArguments,
            handler_args = handlerArgs,
            lambda_params = lambdaParams,
        };

        var template = TemplateHelper.LoadTemplate(LambdaStartupServiceTemplateFile);

        var outCode = template.Render(model);

        context.AddSource("LambdaStartup.g.cs", outCode);
    }
}

internal sealed class DelegateInfo
{
    internal required string ResponseType { get; set; }
    internal required string Namespace { get; set; }
    internal List<ParameterInfo> Parameters { get; set; } = [];
}

internal sealed class ParameterInfo
{
    internal required string ParameterName { get; set; }
    internal required string Type { get; set; }
    internal List<AttributeInfo> Attributes { get; set; } = [];
}

internal sealed class AttributeInfo
{
    internal required string Type { get; set; }
    internal List<string> Arguments { get; set; } = [];
}
