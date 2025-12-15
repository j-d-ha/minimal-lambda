using Microsoft.AspNetCore.Http;

namespace MinimalLambda.Envelopes;

/// <summary>Provides convenience extension methods for common HTTP status codes.</summary>
public static class HttpResultExtensions
{
    extension<TResult>(IHttpResult<TResult>)
        where TResult : IHttpResult<TResult>
    {
        // ── 200 Ok ───────────────────────────────────────────────────────────────────────

        /// <summary>Creates a 200 OK response with content.</summary>
        /// <typeparam name="T">The type of content to return.</typeparam>
        /// <param name="bodyContent">The response content to serialize.</param>
        /// <returns>An HTTP 200 result with JSON content.</returns>
        public static TResult Ok<T>(T bodyContent) =>
            BaseHttpResultExtensions.Json<TResult, T>(StatusCodes.Status200OK, bodyContent);

        /// <summary>Creates a 200 OK response.</summary>
        /// <returns>An HTTP 200 result.</returns>
        public static TResult Ok() =>
            BaseHttpResultExtensions.StatusCode<TResult>(StatusCodes.Status200OK);

        // ── 201 Created ──────────────────────────────────────────────────────────────────

        /// <summary>Creates a 201 Created response.</summary>
        /// <returns>An HTTP 201 result.</returns>
        public static TResult Created() =>
            BaseHttpResultExtensions.StatusCode<TResult>(StatusCodes.Status201Created);

        /// <summary>Creates a 201 Created response with content.</summary>
        /// <typeparam name="T">The type of content to return.</typeparam>
        /// <param name="bodyContent">The response content to serialize.</param>
        /// <returns>An HTTP 201 result with JSON content.</returns>
        public static TResult Created<T>(T bodyContent) =>
            BaseHttpResultExtensions.Json<TResult, T>(StatusCodes.Status201Created, bodyContent);

        // ── 202 Accepted ─────────────────────────────────────────────────────────────────

        /// <summary>Creates a 202 Accepted response.</summary>
        /// <returns>An HTTP 202 result.</returns>
        public static TResult Accepted() =>
            BaseHttpResultExtensions.StatusCode<TResult>(StatusCodes.Status202Accepted);

        /// <summary>Creates a 202 Accepted response with content.</summary>
        /// <typeparam name="T">The type of content to return.</typeparam>
        /// <param name="bodyContent">The response content to serialize.</param>
        /// <returns>An HTTP 202 result with JSON content.</returns>
        public static TResult Accepted<T>(T bodyContent) =>
            BaseHttpResultExtensions.Json<TResult, T>(StatusCodes.Status202Accepted, bodyContent);

        // ── 204 No Content ───────────────────────────────────────────────────────────────

        /// <summary>Creates a 204 No Content response.</summary>
        /// <returns>An HTTP 204 result.</returns>
        public static TResult NoContent() =>
            BaseHttpResultExtensions.StatusCode<TResult>(StatusCodes.Status204NoContent);

        // ── 301 Moved Permanently ────────────────────────────────────────────────────────

        /// <summary>Creates a 301 Moved Permanently response.</summary>
        /// <returns>An HTTP 301 result.</returns>
        public static TResult MovedPermanently() =>
            BaseHttpResultExtensions.StatusCode<TResult>(StatusCodes.Status301MovedPermanently);

        /// <summary>Creates a 301 Moved Permanently response with a location.</summary>
        /// <param name="location">The URI of the redirect target.</param>
        /// <returns>An HTTP 301 result with Location header.</returns>
        public static TResult MovedPermanently(string location) =>
            BaseHttpResultExtensions
                .StatusCode<TResult>(StatusCodes.Status301MovedPermanently)
                .Customize(result => result.Headers["Location"] = location);

        // ── 302 Found ────────────────────────────────────────────────────────────────────

        /// <summary>Creates a 302 Found response.</summary>
        /// <returns>An HTTP 302 result.</returns>
        public static TResult Found() =>
            BaseHttpResultExtensions.StatusCode<TResult>(StatusCodes.Status302Found);

        /// <summary>Creates a 302 Found response with a location.</summary>
        /// <param name="location">The URI of the redirect target.</param>
        /// <returns>An HTTP 302 result with Location header.</returns>
        public static TResult Found(string location) =>
            BaseHttpResultExtensions
                .StatusCode<TResult>(StatusCodes.Status302Found)
                .Customize(result => result.Headers["Location"] = location);

        // ── 400 Bad Request ──────────────────────────────────────────────────────────────

        /// <summary>Creates a 400 Bad Request response.</summary>
        /// <returns>An HTTP 400 result.</returns>
        public static TResult BadRequest() =>
            BaseHttpResultExtensions.StatusCode<TResult>(StatusCodes.Status400BadRequest);

        /// <summary>Creates a 400 Bad Request response with content.</summary>
        /// <typeparam name="T">The type of content to return.</typeparam>
        /// <param name="bodyContent">The response content to serialize.</param>
        /// <returns>An HTTP 400 result with JSON content.</returns>
        public static TResult BadRequest<T>(T bodyContent) =>
            BaseHttpResultExtensions.Json<TResult, T>(StatusCodes.Status400BadRequest, bodyContent);

        // ── 401 Unauthorized ─────────────────────────────────────────────────────────────

        /// <summary>Creates a 401 Unauthorized response.</summary>
        /// <returns>An HTTP 401 result.</returns>
        public static TResult Unauthorized() =>
            BaseHttpResultExtensions.StatusCode<TResult>(StatusCodes.Status401Unauthorized);

        // ── 403 Forbidden ────────────────────────────────────────────────────────────────

        /// <summary>Creates a 403 Forbidden response.</summary>
        /// <returns>An HTTP 403 result.</returns>
        public static TResult Forbidden() =>
            BaseHttpResultExtensions.StatusCode<TResult>(StatusCodes.Status403Forbidden);

        /// <summary>Creates a 403 Forbidden response with content.</summary>
        /// <typeparam name="T">The type of content to return.</typeparam>
        /// <param name="bodyContent">The response content to serialize.</param>
        /// <returns>An HTTP 403 result with JSON content.</returns>
        public static TResult Forbidden<T>(T bodyContent) =>
            BaseHttpResultExtensions.Json<TResult, T>(StatusCodes.Status403Forbidden, bodyContent);

        // ── 404 Not Found ────────────────────────────────────────────────────────────────

        /// <summary>Creates a 404 Not Found response.</summary>
        /// <returns>An HTTP 404 result.</returns>
        public static TResult NotFound() =>
            BaseHttpResultExtensions.StatusCode<TResult>(StatusCodes.Status404NotFound);

        /// <summary>Creates a 404 Not Found response with content.</summary>
        /// <typeparam name="T">The type of content to return.</typeparam>
        /// <param name="bodyContent">The response content to serialize.</param>
        /// <returns>An HTTP 404 result with JSON content.</returns>
        public static TResult NotFound<T>(T bodyContent) =>
            BaseHttpResultExtensions.Json<TResult, T>(StatusCodes.Status404NotFound, bodyContent);

        // ── 409 Conflict ─────────────────────────────────────────────────────────────────

        /// <summary>Creates a 409 Conflict response.</summary>
        /// <returns>An HTTP 409 result.</returns>
        public static TResult Conflict() =>
            BaseHttpResultExtensions.StatusCode<TResult>(StatusCodes.Status409Conflict);

        /// <summary>Creates a 409 Conflict response with content.</summary>
        /// <typeparam name="T">The type of content to return.</typeparam>
        /// <param name="bodyContent">The response content to serialize.</param>
        /// <returns>An HTTP 409 result with JSON content.</returns>
        public static TResult Conflict<T>(T bodyContent) =>
            BaseHttpResultExtensions.Json<TResult, T>(StatusCodes.Status409Conflict, bodyContent);

        // ── 422 Unprocessable Entity ─────────────────────────────────────────────────────

        /// <summary>Creates a 422 Unprocessable Entity response.</summary>
        /// <returns>An HTTP 422 result.</returns>
        public static TResult UnprocessableEntity() =>
            BaseHttpResultExtensions.StatusCode<TResult>(StatusCodes.Status422UnprocessableEntity);

        /// <summary>Creates a 422 Unprocessable Entity response with content.</summary>
        /// <typeparam name="T">The type of content to return.</typeparam>
        /// <param name="bodyContent">The response content to serialize.</param>
        /// <returns>An HTTP 422 result with JSON content.</returns>
        public static TResult UnprocessableEntity<T>(T bodyContent) =>
            BaseHttpResultExtensions.Json<TResult, T>(
                StatusCodes.Status422UnprocessableEntity,
                bodyContent
            );

        // ── 500 Internal Server Error ────────────────────────────────────────────────────

        /// <summary>Creates a 500 Internal Server Error response.</summary>
        /// <returns>An HTTP 500 result.</returns>
        public static TResult InternalServerError() =>
            BaseHttpResultExtensions.StatusCode<TResult>(StatusCodes.Status500InternalServerError);

        /// <summary>Creates a 500 Internal Server Error response with content.</summary>
        /// <typeparam name="T">The type of content to return.</typeparam>
        /// <param name="bodyContent">The response content to serialize.</param>
        /// <returns>An HTTP 500 result with JSON content.</returns>
        public static TResult InternalServerError<T>(T bodyContent) =>
            BaseHttpResultExtensions.Json<TResult, T>(
                StatusCodes.Status500InternalServerError,
                bodyContent
            );
    }
}
