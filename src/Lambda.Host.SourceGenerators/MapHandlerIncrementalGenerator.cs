using Lambda.Host.SourceGenerators.Models;
using Lambda.Host.SourceGenerators.Types;
using Microsoft.CodeAnalysis;

namespace Lambda.Host.SourceGenerators;

[Generator]
public class MapHandlerIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find all partial classes with LambdaHost attribute
        var startupClassInfo = context
            .SyntaxProvider.ForAttributeWithMetadataName(
                AttributeConstants.LambdaHostAttribute,
                LambdaHostSyntaxProvider.Predicate,
                LambdaHostSyntaxProvider.Transformer
            )
            .Where(static m => m is not null)
            .Select(static (m, _) => m!.Value);

        // Find all MapHandler method calls with lambda analysis
        var mapHandlerCalls = context
            .SyntaxProvider.CreateSyntaxProvider(
                MapHandlerSyntaxProvider.Predicate,
                MapHandlerSyntaxProvider.Transformer
            )
            .Where(static m => m is not null)
            .Select(static (m, _) => m!.Value);

        // combine the compilation and map handler calls
        var combined = mapHandlerCalls
            .Collect()
            .Combine(startupClassInfo.Collect())
            .Select(
                (t, _) => new CompilationInfo(t.Left.ToEquatableArray(), t.Right.ToEquatableArray())
            );

        // Generate source when calls are found
        context.RegisterSourceOutput(combined, MapHandlerSourceOutput.Generate);
    }
}
