using System.Linq;
using System.Text;
using MinimalLambda.SourceGenerators.Models;

namespace MinimalLambda.SourceGenerators.Extensions;

internal static class DelegateInfoExtensions
{
    internal static string BuildHandlerSignature(this DelegateInfo delegateInfo)
    {
        // build handler function signature
        var signatureBuilder = new StringBuilder();
        signatureBuilder.Append(delegateInfo.DelegateType);

        // angle brackets needed for any parameters or a response type
        if (
            delegateInfo.Parameters.Count > 0
            || delegateInfo.ReturnTypeInfo.FullyQualifiedType != TypeConstants.Void
        )
        {
            signatureBuilder.Append("<");

            // join parameters with comma
            if (delegateInfo.Parameters.Count > 0)
                signatureBuilder.Append(
                    string.Join(
                        ", ",
                        delegateInfo.Parameters.Select(p => p.TypeInfo.FullyQualifiedType)
                    )
                );

            if (delegateInfo.ReturnTypeInfo.FullyQualifiedType != TypeConstants.Void)
            {
                // add comma if there are parameters, i.e. this is the last in the list
                if (delegateInfo.Parameters.Count > 0)
                    signatureBuilder.Append(", ");
                signatureBuilder.Append(delegateInfo.ReturnTypeInfo.FullyQualifiedType);
            }

            signatureBuilder.Append(">");
        }

        var handlerSignature = signatureBuilder.ToString();

        return handlerSignature;
    }
}
