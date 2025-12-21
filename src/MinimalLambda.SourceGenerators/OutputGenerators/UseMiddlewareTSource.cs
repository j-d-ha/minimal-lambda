using System.Linq;
using MinimalLambda.SourceGenerators.Models;
using MinimalLambda.SourceGenerators.Types;

namespace MinimalLambda.SourceGenerators;

internal static class UseMiddlewareTSource
{
    internal static string Generate(
        EquatableArray<UseMiddlewareTInfo> useMiddlewareTInfos,
        string generatedCodeAttribute
    )
    {
        var useMiddlewareTCalls = useMiddlewareTInfos.Select(useMiddlewareTInfo =>
        {
            var classInfo = useMiddlewareTInfo.ClassInfo;

            // choose what constructor to use with the following criteria:
            // 1. if it has a `[MiddlewareConstructor]` attribute. Multiple of these are not valid.
            // 2. default to the constructor with the most arguments
            var constructor = classInfo
                .ConstructorInfos.Select(c => (MethodInfo?)c)
                .FirstOrDefault(c =>
                    c!.Value.AttributeInfos.Any(a =>
                        a.FullName == AttributeConstants.MiddlewareConstructor
                    )
                );

            constructor ??= classInfo
                .ConstructorInfos.OrderByDescending(c => c.ArgumentCount)
                .First();

            var parameters = constructor
                .Value.Parameters.Select(p =>
                {
                    var fromArgs = p.AttributeNames.Any(n => n == AttributeConstants.FromArguments);

                    // From services is defined as either having a `[FromServices]` attribute or a
                    // `[FromKeyedServices]` attribute
                    var fromServices = p.AttributeNames.Any(n =>
                        n is AttributeConstants.FromServices or AttributeConstants.FromKeyedService
                    );

                    var paramAssignment = p.BuildParameterAssignment();

                    var fullyQualifiedTypeNotNull =
                        p.TypeInfo.FullyQualifiedType.RemoveTrailingChar("?");

                    return new
                    {
                        p.TypeInfo.FullyQualifiedType,
                        FullyQualifiedTypeNotNull = fullyQualifiedTypeNotNull,
                        p.Name,
                        FromArguments = fromArgs,
                        FromServices = fromServices,
                        paramAssignment.Assignment,
                        paramAssignment.String,
                    };
                })
                .ToArray();

            var isDisposable = useMiddlewareTInfo.ClassInfo.IsInterfaceImplemented(
                TypeConstants.IDisposable
            );

            var isAsyncDisposable = useMiddlewareTInfo.ClassInfo.IsInterfaceImplemented(
                TypeConstants.IAsyncDisposable
            );

            var allFromServices = parameters.All(p => p.FromServices);

            return new
            {
                Location = useMiddlewareTInfo.InterceptableLocationInfo,
                FullMiddlewareClassName = classInfo.GloballyQualifiedName,
                ShortMiddlewareClassName = classInfo.ShortName,
                AllFromServices = allFromServices,
                Parameters = parameters,
                AnyParameters = parameters.Length > 0,
                IsDisposable = isDisposable,
                IsAsyncDisposable = isAsyncDisposable,
            };
        });

        var template = TemplateHelper.LoadTemplate(GeneratorConstants.UseMiddlewareTTemplateFile);

        return template.Render(
            new { GeneratedCodeAttribute = generatedCodeAttribute, Calls = useMiddlewareTCalls }
        );
    }

    private static ParameterArg BuildParameterAssignment(this ParameterInfo param) =>
        new()
        {
            String = param.ToPublicString(),
            Assignment = param.Source switch
            {
                // inject keyed service from the DI container - required
                ParameterSource.KeyedService when param.IsRequired =>
                    $"context.ServiceProvider.GetRequiredKeyedService<{param.TypeInfo.FullyQualifiedType}>({param.KeyedServiceKey?.DisplayValue})",

                // inject keyed service from the DI container - optional
                ParameterSource.KeyedService =>
                    $"context.ServiceProvider.GetKeyedService<{param.TypeInfo.FullyQualifiedType}>({param.KeyedServiceKey?.DisplayValue})",

                // default: inject service from the DI container - required
                _ when param.IsRequired =>
                    $"context.ServiceProvider.GetRequiredService<{param.TypeInfo.FullyQualifiedType}>()",

                // default: inject service from the DI container - optional
                _ => $"context.ServiceProvider.GetService<{param.TypeInfo.FullyQualifiedType}>()",
            },
        };

    private static string RemoveTrailingChar(this string value, string trailing) =>
        value.EndsWith(trailing) ? value[..^1] : value;

    private readonly record struct ParameterArg(string String, string Assignment);
}
