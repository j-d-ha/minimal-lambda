using Microsoft.AspNetCore.Http;

namespace MinimalLambda.Envelopes.ApiGateway;

public static class HttpResultExtensions
{
    extension<THttpResult>(IHttpResult<THttpResult>)
        where THttpResult : IHttpResult<THttpResult>
    {
        // ── 200 Ok ───────────────────────────────────────────────────────────────────────

        public static THttpResult Ok<T>(T bodyContent) =>
            BaseHttpResultExtensions.Json<THttpResult, T>(StatusCodes.Status200OK, bodyContent);

        public static THttpResult Ok() =>
            BaseHttpResultExtensions.StatusCode<THttpResult>(StatusCodes.Status200OK);

        // ── 201 Created ──────────────────────────────────────────────────────────────────

        public static THttpResult Created() =>
            BaseHttpResultExtensions.StatusCode<THttpResult>(StatusCodes.Status201Created);

        public static THttpResult Created<T>(T bodyContent) =>
            BaseHttpResultExtensions.Json<THttpResult, T>(
                StatusCodes.Status201Created,
                bodyContent
            );

        // ── 204 No Content ───────────────────────────────────────────────────────────────

        public static THttpResult NoContent() =>
            BaseHttpResultExtensions.StatusCode<THttpResult>(StatusCodes.Status204NoContent);

        // ── 400 Bad Request ──────────────────────────────────────────────────────────────

        public static THttpResult BadRequest() =>
            BaseHttpResultExtensions.StatusCode<THttpResult>(StatusCodes.Status400BadRequest);

        public static THttpResult BadRequest<T>(T bodyContent) =>
            BaseHttpResultExtensions.Json<THttpResult, T>(
                StatusCodes.Status400BadRequest,
                bodyContent
            );

        // ── 401 Unauthorized ─────────────────────────────────────────────────────────────

        public static THttpResult Unauthorized() =>
            BaseHttpResultExtensions.StatusCode<THttpResult>(StatusCodes.Status401Unauthorized);

        // ── 404 Not Found ────────────────────────────────────────────────────────────────

        public static THttpResult NotFound() =>
            BaseHttpResultExtensions.StatusCode<THttpResult>(StatusCodes.Status404NotFound);

        public static THttpResult NotFound<T>(T bodyContent) =>
            BaseHttpResultExtensions.Json<THttpResult, T>(
                StatusCodes.Status404NotFound,
                bodyContent
            );

        // ── 409 Conflict ─────────────────────────────────────────────────────────────────

        public static THttpResult Conflict() =>
            BaseHttpResultExtensions.StatusCode<THttpResult>(StatusCodes.Status409Conflict);

        public static THttpResult Conflict<T>(T bodyContent) =>
            BaseHttpResultExtensions.Json<THttpResult, T>(
                StatusCodes.Status409Conflict,
                bodyContent
            );

        // ── 422 Unprocessable Entity ─────────────────────────────────────────────────────

        public static THttpResult UnprocessableEntity() =>
            BaseHttpResultExtensions.StatusCode<THttpResult>(
                StatusCodes.Status422UnprocessableEntity
            );

        public static THttpResult UnprocessableEntity<T>(T bodyContent) =>
            BaseHttpResultExtensions.Json<THttpResult, T>(
                StatusCodes.Status422UnprocessableEntity,
                bodyContent
            );

        // ── 500 Internal Server Error ────────────────────────────────────────────────────

        public static THttpResult InternalServerError() =>
            BaseHttpResultExtensions.StatusCode<THttpResult>(
                StatusCodes.Status500InternalServerError
            );

        public static THttpResult InternalServerError<T>(T bodyContent) =>
            BaseHttpResultExtensions.Json<THttpResult, T>(
                StatusCodes.Status500InternalServerError,
                bodyContent
            );
    }
}
