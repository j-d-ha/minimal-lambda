using Lambda.Host.SourceGenerators.Models;
using Microsoft.CodeAnalysis;

namespace Lambda.Host.SourceGenerators;

[Generator]
public class MapHandlerIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var startupClassInfo = context
            .SyntaxProvider.ForAttributeWithMetadataName(
                AttributeConstants.LambdaHostAttribute,
                LambdaHostSyntaxProvider.Predicate,
                LambdaHostSyntaxProvider.Transformer
            )
            .Where(static m => m is not null);

        // Find all MapHandler method calls with lambda analysis
        var mapHandlerCalls = context
            .SyntaxProvider.CreateSyntaxProvider(
                MapHandlerSyntaxProvider.Predicate,
                MapHandlerSyntaxProvider.Transformer
            )
            .Where(static m => m is not null)
            .Select(static (m, _) => m!);

        // combine the compilation and map handler calls
        var combined = mapHandlerCalls
            .Collect()
            .Combine(startupClassInfo.Collect())
            .Select(
                (t, _) =>
                    new CompilationInfo
                    {
                        MapHandlerInvocationInfos = t.Left,
                        StartupClassInfos = t.Right,
                    }
            );

        // Generate source when calls are found
        context.RegisterSourceOutput(combined, MapHandlerSourceOutput.Generate);
    }
}
