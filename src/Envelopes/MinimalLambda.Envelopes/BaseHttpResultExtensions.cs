namespace MinimalLambda.Envelopes;

/// <summary>Provides factory extension methods for creating HTTP results.</summary>
public static class BaseHttpResultExtensions
{
    extension<TResult>(IHttpResult<TResult>)
        where TResult : IHttpResult<TResult>
    {
        /// <summary>Creates an HTTP result with the specified status code.</summary>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <returns>An HTTP result with the status code.</returns>
        public static TResult StatusCode(int statusCode) =>
            TResult.Create<object?>(statusCode, null, new Dictionary<string, string>(), false);

        /// <summary>Creates a text/plain HTTP result.</summary>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <param name="body">The plain text response body.</param>
        /// <returns>An HTTP result with text/plain content type.</returns>
        public static TResult Text(int statusCode, string body) =>
            TResult
                .Create<object?>(
                    statusCode,
                    null,
                    new Dictionary<string, string>
                    {
                        ["Content-Type"] = "text/plain; charset=utf-8",
                    },
                    false
                )
                .Customize(result => result.Body = body);

        /// <summary>Creates an application/json HTTP result.</summary>
        /// <typeparam name="T">The type of content to serialize.</typeparam>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <param name="bodyContent">The content to serialize as JSON.</param>
        /// <returns>An HTTP result with application/json content type.</returns>
        public static TResult Json<T>(int statusCode, T bodyContent) =>
            TResult.Create(
                statusCode,
                bodyContent,
                new Dictionary<string, string>
                {
                    ["Content-Type"] = "application/json; charset=utf-8",
                },
                false
            );
    }

    extension<TResult>(TResult result)
        where TResult : IHttpResult<TResult>
    {
        /// <summary>Applies customizations to the result.</summary>
        /// <param name="customizer">An action to customize the result properties.</param>
        /// <returns>The same instance for method chaining.</returns>
        public TResult Customize(Action<TResult> customizer)
        {
            customizer(result);
            return result;
        }
    }
}
