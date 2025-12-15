using Microsoft.AspNetCore.Http;

namespace MinimalLambda.Envelopes;

/// <summary>Provides convenience extension methods for common HTTP status codes.</summary>
public static class HttpResultExtensions
{
    extension<THttpResult>(IHttpResult<THttpResult>)
        where THttpResult : IHttpResult<THttpResult>
    {
        // ── 200 Ok ───────────────────────────────────────────────────────────────────────

        /// <summary>Creates a 200 OK response with content.</summary>
        /// <typeparam name="T">The type of content to return.</typeparam>
        /// <param name="bodyContent">The response content to serialize.</param>
        /// <returns>An HTTP 200 result with JSON content.</returns>
        public static THttpResult Ok<T>(T bodyContent) =>
            BaseHttpResultExtensions.Json<THttpResult, T>(StatusCodes.Status200OK, bodyContent);

        /// <summary>Creates a 200 OK response.</summary>
        /// <returns>An HTTP 200 result.</returns>
        public static THttpResult Ok() =>
            BaseHttpResultExtensions.StatusCode<THttpResult>(StatusCodes.Status200OK);

        // ── 201 Created ──────────────────────────────────────────────────────────────────

        /// <summary>Creates a 201 Created response.</summary>
        /// <returns>An HTTP 201 result.</returns>
        public static THttpResult Created() =>
            BaseHttpResultExtensions.StatusCode<THttpResult>(StatusCodes.Status201Created);

        /// <summary>Creates a 201 Created response with content.</summary>
        /// <typeparam name="T">The type of content to return.</typeparam>
        /// <param name="bodyContent">The response content to serialize.</param>
        /// <returns>An HTTP 201 result with JSON content.</returns>
        public static THttpResult Created<T>(T bodyContent) =>
            BaseHttpResultExtensions.Json<THttpResult, T>(
                StatusCodes.Status201Created,
                bodyContent
            );

        // ── 204 No Content ───────────────────────────────────────────────────────────────

        /// <summary>Creates a 204 No Content response.</summary>
        /// <returns>An HTTP 204 result.</returns>
        public static THttpResult NoContent() =>
            BaseHttpResultExtensions.StatusCode<THttpResult>(StatusCodes.Status204NoContent);

        // ── 400 Bad Request ──────────────────────────────────────────────────────────────

        /// <summary>Creates a 400 Bad Request response.</summary>
        /// <returns>An HTTP 400 result.</returns>
        public static THttpResult BadRequest() =>
            BaseHttpResultExtensions.StatusCode<THttpResult>(StatusCodes.Status400BadRequest);

        /// <summary>Creates a 400 Bad Request response with content.</summary>
        /// <typeparam name="T">The type of content to return.</typeparam>
        /// <param name="bodyContent">The response content to serialize.</param>
        /// <returns>An HTTP 400 result with JSON content.</returns>
        public static THttpResult BadRequest<T>(T bodyContent) =>
            BaseHttpResultExtensions.Json<THttpResult, T>(
                StatusCodes.Status400BadRequest,
                bodyContent
            );

        // ── 401 Unauthorized ─────────────────────────────────────────────────────────────

        /// <summary>Creates a 401 Unauthorized response.</summary>
        /// <returns>An HTTP 401 result.</returns>
        public static THttpResult Unauthorized() =>
            BaseHttpResultExtensions.StatusCode<THttpResult>(StatusCodes.Status401Unauthorized);

        // ── 404 Not Found ────────────────────────────────────────────────────────────────

        /// <summary>Creates a 404 Not Found response.</summary>
        /// <returns>An HTTP 404 result.</returns>
        public static THttpResult NotFound() =>
            BaseHttpResultExtensions.StatusCode<THttpResult>(StatusCodes.Status404NotFound);

        /// <summary>Creates a 404 Not Found response with content.</summary>
        /// <typeparam name="T">The type of content to return.</typeparam>
        /// <param name="bodyContent">The response content to serialize.</param>
        /// <returns>An HTTP 404 result with JSON content.</returns>
        public static THttpResult NotFound<T>(T bodyContent) =>
            BaseHttpResultExtensions.Json<THttpResult, T>(
                StatusCodes.Status404NotFound,
                bodyContent
            );

        // ── 409 Conflict ─────────────────────────────────────────────────────────────────

        /// <summary>Creates a 409 Conflict response.</summary>
        /// <returns>An HTTP 409 result.</returns>
        public static THttpResult Conflict() =>
            BaseHttpResultExtensions.StatusCode<THttpResult>(StatusCodes.Status409Conflict);

        /// <summary>Creates a 409 Conflict response with content.</summary>
        /// <typeparam name="T">The type of content to return.</typeparam>
        /// <param name="bodyContent">The response content to serialize.</param>
        /// <returns>An HTTP 409 result with JSON content.</returns>
        public static THttpResult Conflict<T>(T bodyContent) =>
            BaseHttpResultExtensions.Json<THttpResult, T>(
                StatusCodes.Status409Conflict,
                bodyContent
            );

        // ── 422 Unprocessable Entity ─────────────────────────────────────────────────────

        /// <summary>Creates a 422 Unprocessable Entity response.</summary>
        /// <returns>An HTTP 422 result.</returns>
        public static THttpResult UnprocessableEntity() =>
            BaseHttpResultExtensions.StatusCode<THttpResult>(
                StatusCodes.Status422UnprocessableEntity
            );

        /// <summary>Creates a 422 Unprocessable Entity response with content.</summary>
        /// <typeparam name="T">The type of content to return.</typeparam>
        /// <param name="bodyContent">The response content to serialize.</param>
        /// <returns>An HTTP 422 result with JSON content.</returns>
        public static THttpResult UnprocessableEntity<T>(T bodyContent) =>
            BaseHttpResultExtensions.Json<THttpResult, T>(
                StatusCodes.Status422UnprocessableEntity,
                bodyContent
            );

        // ── 500 Internal Server Error ────────────────────────────────────────────────────

        /// <summary>Creates a 500 Internal Server Error response.</summary>
        /// <returns>An HTTP 500 result.</returns>
        public static THttpResult InternalServerError() =>
            BaseHttpResultExtensions.StatusCode<THttpResult>(
                StatusCodes.Status500InternalServerError
            );

        /// <summary>Creates a 500 Internal Server Error response with content.</summary>
        /// <typeparam name="T">The type of content to return.</typeparam>
        /// <param name="bodyContent">The response content to serialize.</param>
        /// <returns>An HTTP 500 result with JSON content.</returns>
        public static THttpResult InternalServerError<T>(T bodyContent) =>
            BaseHttpResultExtensions.Json<THttpResult, T>(
                StatusCodes.Status500InternalServerError,
                bodyContent
            );
    }
}
