using System.Linq;
using AwsLambda.Host.SourceGenerators.Extensions;
using AwsLambda.Host.SourceGenerators.Models;
using AwsLambda.Host.SourceGenerators.Types;

namespace AwsLambda.Host.SourceGenerators;

internal static class GenericHandlerSources
{
    /// <summary>
    ///     Generates C# code for a generic handler. The handler is a wrapper around the actual
    ///     handler. The return type of the wrapper is a <c>Task</c> or <c>Task&lt;T&gt;</c> depending on
    ///     the return type of the actual handler.
    /// </summary>
    /// <param name="higherOrderMethodInfos">The method info from which the handler is generated</param>
    /// <param name="methodName">
    ///     The name of the method that the handler is coming from and for which the
    ///     code is intercepting
    /// </param>
    /// <param name="wrapperReturnType">
    ///     The return type of the wrapper function, without Task. Ex.,
    ///     <c>bool</c> not <c>Task&lt;bool&gt;</c>. A <c>null</c> signifies no return value.
    /// </param>
    /// <param name="defaultWrapperReturnValue">
    ///     The default value the wrapped handler will return if the
    ///     handler does not return a value of the same type as wrapperReturnType. This is without Task.
    ///     Ex., <c>bool</c> not <c>Task&lt;bool&gt;</c>. A <c>null</c> signifies no default return value.
    /// </param>
    /// <returns></returns>
    internal static string Generate(
        EquatableArray<HigherOrderMethodInfo> higherOrderMethodInfos,
        string methodName,
        string? wrapperReturnType,
        string? defaultWrapperReturnValue
    )
    {
        var calls = higherOrderMethodInfos
            .Select(higherOrderMethodInfo =>
            {
                // build handler function signature
                var handlerSignature = higherOrderMethodInfo.DelegateInfo.BuildHandlerSignature();

                // get arguments for handler
                var handlerArgs =
                    higherOrderMethodInfo.DelegateInfo.BuildHandlerParameterAssignment();

                // get the return type of the wrapper function wrapped in a Task
                var fullWrapperReturnType = wrapperReturnType is not null
                    ? $"global::System.Threading.Tasks.Task<{wrapperReturnType}>"
                    : "global::System.Threading.Tasks.Task";

                // get the return type of the wrapper function wrapped in a Task - shortened to just use Task
                var shortFullWrapperReturnType = wrapperReturnType is not null
                    ? $"Task<{wrapperReturnType}>"
                    : "Task";

                // should await determined by whether the delegate is awaitable and if the delegate
                // return type matches the wrapper return type 1:1
                var shouldAwait =
                    fullWrapperReturnType != higherOrderMethodInfo.DelegateInfo.FullResponseType
                    && higherOrderMethodInfo.DelegateInfo.IsAwaitable;

                // should return response
                var shouldReturnResponse =
                    wrapperReturnType == higherOrderMethodInfo.DelegateInfo.UnwrappedResponseType
                    || fullWrapperReturnType == higherOrderMethodInfo.DelegateInfo.FullResponseType;

                // should wrap the response in a Task
                var shouldWrapResponse =
                    shouldReturnResponse && !higherOrderMethodInfo.DelegateInfo.IsAwaitable;

                // default return value
                var defaultReturnValueString = !shouldAwait
                    ? defaultWrapperReturnValue is not null
                        ? $"Task.FromResult({defaultWrapperReturnValue})"
                        : "Task.CompletedTask"
                    : defaultWrapperReturnValue;

                return new
                {
                    Location = higherOrderMethodInfo.InterceptableLocationInfo,
                    WrapperReturnType = shortFullWrapperReturnType,
                    HandlerSignature = handlerSignature,
                    ShouldAwait = shouldAwait,
                    higherOrderMethodInfo.DelegateInfo.HasAnyKeyedServiceParameter,
                    HandlerArgs = handlerArgs,
                    ShouldReturnResponse = shouldReturnResponse,
                    ShouldWrapResponse = shouldWrapResponse,
                    DefaultReturnValue = defaultReturnValueString,
                };
            })
            .ToArray();

        var model = new { Name = methodName, Calls = calls };

        var template = TemplateHelper.LoadTemplate(GeneratorConstants.GenericHandlerTemplateFile);

        var outCode = template.Render(model);

        return outCode;
    }

    private static HandlerArg[] BuildHandlerParameterAssignment(this DelegateInfo delegateInfo)
    {
        var handlerArgs = delegateInfo
            .Parameters.Select(param => new HandlerArg
            {
                String = param.ToPublicString(),
                Assignment = param.Source switch
                {
                    // CancellationToken -> get directly from arguments
                    ParameterSource.CancellationToken => "cancellationToken",

                    // inject keyed service from the DI container - required
                    ParameterSource.KeyedService when param.IsRequired =>
                        $"serviceProvider.GetRequiredKeyedService<{param.Type}>({param.KeyedServiceKey?.DisplayValue})",

                    // inject keyed service from the DI container - optional
                    ParameterSource.KeyedService =>
                        $"serviceProvider.GetKeyedService<{param.Type}>({param.KeyedServiceKey?.DisplayValue})",

                    // default: inject service from the DI container - required
                    _ when param.IsRequired =>
                        $"serviceProvider.GetRequiredService<{param.Type}>()",

                    // default: inject service from the DI container - optional
                    _ => $"serviceProvider.GetService<{param.Type}>()",
                },
            })
            .ToArray();

        return handlerArgs;
    }

    private readonly record struct HandlerArg(string String, string Assignment);
}
