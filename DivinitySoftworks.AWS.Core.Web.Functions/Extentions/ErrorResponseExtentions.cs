using Amazon.Lambda.Annotations.APIGateway;
using System.Net;

using static Amazon.Lambda.Annotations.APIGateway.HttpResults;

namespace DivinitySoftworks.Core.Web.Errors;

/// <summary>
/// Provides extension methods for converting an <see cref="ErrorResponse"/> into HTTP result types.
/// </summary>
public static class ErrorResponseExtensions {
    /// <summary>
    /// Converts an <see cref="ErrorResponse"/> into an <see cref="IHttpResult"/> based on the HTTP status code.
    /// </summary>
    /// <param name="errorResponse">The <see cref="ErrorResponse"/> object containing details about the error.</param>
    /// <returns>An <see cref="IHttpResult"/> that represents the appropriate HTTP response for the given error.</returns>
    public static IHttpResult ToHttpResult(this ErrorResponse errorResponse) {
        return errorResponse.StatusCode switch {
            HttpStatusCode.BadRequest => BadRequest(errorResponse),
            HttpStatusCode.Unauthorized => NewResult(HttpStatusCode.Unauthorized, errorResponse),
            HttpStatusCode.Forbidden => NewResult(HttpStatusCode.Forbidden, errorResponse),
            HttpStatusCode.NotFound => NotFound(errorResponse),
            HttpStatusCode.InternalServerError => InternalServerError(errorResponse),
            HttpStatusCode.ServiceUnavailable => NewResult(HttpStatusCode.ServiceUnavailable, errorResponse),
            _ => NewResult(errorResponse.StatusCode, errorResponse)
        };
    }
}
