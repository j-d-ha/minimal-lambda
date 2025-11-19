using AwsLambda.Host.Core.Features;
using AwsLambda.Host.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AwsLambda.Host;

/// <summary>Provides middleware for processing Lambda event and response envelopes.</summary>
public static class RequestEnvelopeMiddleware
{
    extension(ILambdaInvocationBuilder application)
    {
        /// <summary>Adds envelope extraction and packing middleware to the lambda invocation pipeline.</summary>
        /// <remarks>
        ///     <para>
        ///         This middleware automatically processes Lambda events and responses that implement
        ///         <see cref="IRequestEnvelope" /> and <see cref="IResponseEnvelope" /> respectively.
        ///     </para>
        ///     <para>
        ///         During request processing, if the Lambda event implements <see cref="IRequestEnvelope" />
        ///         , the middleware calls <see cref="IRequestEnvelope.ExtractPayload" /> before the event is
        ///         passed to the handler. After the handler completes, if the response implements
        ///         <see cref="IResponseEnvelope" />, the middleware calls
        ///         <see cref="IResponseEnvelope.PackPayload" /> before returning the response to the Lambda
        ///         runtime.
        ///     </para>
        /// </remarks>
        /// <returns>The <see cref="ILambdaApplication" /> for method chaining.</returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <see cref="ILambdaApplication" /> is
        ///     <c>null</c>.
        /// </exception>
        /// <seealso cref="IRequestEnvelope" />
        /// <seealso cref="IResponseEnvelope" />
        /// <seealso cref="EnvelopeOptions" />
        public ILambdaInvocationBuilder UseExtractAndPackEnvelope()
        {
            ArgumentNullException.ThrowIfNull(application);

            var settings = application
                .Services.GetRequiredService<IOptions<EnvelopeOptions>>()
                .Value;

            application.Use(next =>
            {
                return async context =>
                {
                    if (
                        context.Features.TryGet(out IEventFeature? eventFeature)
                        && eventFeature.GetEvent(context) is IRequestEnvelope requestEnvelope
                    )
                        requestEnvelope.ExtractPayload(settings);

                    await next(context);

                    if (
                        context.Features.TryGet(out IResponseFeature? responseFeature)
                        && responseFeature.GetResponse() is IResponseEnvelope responseEnvelope
                    )
                        responseEnvelope.PackPayload(settings);
                };
            });

            return application;
        }
    }
}
