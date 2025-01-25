using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using DivinitySoftworks.Core.Web.Errors;
using DivinitySoftworks.Core.Web.Security;
using Microsoft.Extensions.Logging;
using System.Net;

using static Amazon.Lambda.Annotations.APIGateway.HttpResults;

namespace DivinitySoftworks.AWS.Core.Web.Functions;

/// <summary>
/// Provides a base class for executable functions with authorization handling.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ExecutableFunction"/> class.
/// </remarks>
/// <param name="authorizeService">The service used to handle authorization.</param>
public class ExecutableFunction(IAuthorizeService authorizeService) {
    protected readonly IAuthorizeService _authorizeService = authorizeService;

    /// <summary>
    /// Asynchronously authorizes a request based on the provided bearer token.
    /// </summary>
    /// <param name="bearer">The bearer token for authorization.</param>
    /// <returns>A task representing the authorization result.</returns>
    protected virtual Task<AuthorizeResult> AuthorizeAsync(string bearer) {
        return _authorizeService.Authorize(bearer);
    }

    /// <summary>
    /// Executes the specified asynchronous function with authorization and error handling.
    /// </summary>
    /// <param name="authorize">The type of authorization required.</param>
    /// <param name="context">The AWS Lambda context for the current invocation.</param>
    /// <param name="apiKey">The API key provided for authorization.</param>
    /// <param name="input">The request object containing the API key to validate against.</param>
    /// <param name="function">The asynchronous function to be executed.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// If authorization is required and the provided API key does not match the key in the request, an error is logged and the function does not execute.
    /// If an exception is thrown during the execution of the function, an error message is logged.
    /// </remarks>
    public async Task ExecuteAsync(Authorize authorize, ILambdaContext context, string apiKey, IApiKey input, Func<Task> function) {
        try {
            if (authorize == Authorize.Unknown)
                throw new Exception("The authorize type is unknown.");

            if (authorize == Authorize.Required && apiKey != input.Value) {
                context.Logger.LogError($"The API Key is invalid.");
                return;
            }

            await function();
        }
        catch (Exception exception) {
            context.Logger.LogError(exception, "Exception thrown while executing the function: {Message}", exception.Message);
        }
    }

    /// <summary>
    /// Executes the specified asynchronous function with authorization and error handling.
    /// </summary>
    /// <param name="authorize">The type of authorization required.</param>
    /// <param name="context">The AWS Lambda context for the current invocation.</param>
    /// <param name="request">The API Gateway HTTP API V2 proxy request.</param>
    /// <param name="function">The asynchronous function to be executed, which returns an <see cref="IHttpResult"/>.</param>
    /// <returns>A task that represents the asynchronous operation, containing the <see cref="IHttpResult"/> from the executed function or an error response if an exception occurs.</returns>
    /// <remarks>
    /// If authorization is required, the function checks the request headers for an authorization token and validates it. 
    /// If the token is invalid, an internal server error response is returned. 
    /// If an exception is thrown during the execution of the function, an error response is logged and returned.
    /// An "X-DS-Token" header with a new GUID is added to the response if the result is of type <see cref="HttpResults"/>.
    /// </remarks>
    public async Task<IHttpResult> ExecuteAsync(Authorize authorize, ILambdaContext context, APIGatewayHttpApiV2ProxyRequest request, Func<Task<IHttpResult>> function) {
        IHttpResult? httpResult = null;
        try {
            LogRequestDetails(request, context);

            if (authorize == Authorize.Unknown)
                throw new Exception("The authorize type is unknown.");

            if (authorize == Authorize.Required) {
                // Headers are lowercase.
                if (!request.Headers.TryGetValue("authorization", out string? authorizationHeader)) {
                    context.Logger.LogWarning("Authorization header is missing.");
                    authorizationHeader = string.Empty;
                }

                AuthorizeResult authorizeResult = await AuthorizeAsync(authorizationHeader);

                if (authorizeResult.StatusCode != HttpStatusCode.OK && authorizeResult.StatusCode != HttpStatusCode.Continue) {
                    context.Logger.LogWarning("Authorization failed: {Error}: {ErrorMessage}", authorizeResult.Error, authorizeResult.ErrorMessage);
                    httpResult = new ErrorResponse(authorizeResult.StatusCode ?? HttpStatusCode.InternalServerError, authorizeResult.Error, authorizeResult.ErrorMessage).ToHttpResult();
                }
            }

            httpResult ??= await function();
        }
        catch (Exception exception) {
            context.Logger.LogError(exception, "Exception thrown while executing the function: {Message}", exception.Message);
            httpResult = new ErrorResponse(exception).ToHttpResult();
        }

        if (httpResult is HttpResults httpResults)
            httpResults.AddHeader("x-ds-token", $"{Guid.NewGuid()}");

        return httpResult;
    }

    /// <summary>
    /// Creates an <see cref="IHttpResult"/> representing a No Content (204) status code response.
    /// </summary>
    /// <param name="body">
    /// An optional response body. While the HTTP 204 status code typically indicates no content, 
    /// a body can still be included depending on the API design. If not provided, the response 
    /// will have no additional body content.
    /// </param>
    /// <returns>
    /// An <see cref="IHttpResult"/> with a status code of <see cref="HttpStatusCode.NoContent"/>.
    /// </returns>
    protected static IHttpResult NoContent(object? body = null) {
        return NewResult(HttpStatusCode.NoContent, body);
    }

    /// <summary>
    /// Creates an <see cref="IHttpResult"/> representing an Unauthorized (401) status code response.
    /// </summary>
    /// <param name="body">
    /// An optional response body containing additional details about the unauthorized error. 
    /// If not provided, the response will contain no additional body content.
    /// </param>
    /// <returns>
    /// An <see cref="IHttpResult"/> with a status code of <see cref="HttpStatusCode.Unauthorized"/>.
    /// </returns>
    protected static IHttpResult Unauthorized(object? body = null) {
        return NewResult(HttpStatusCode.Unauthorized, body);
    }

    /// <summary>
    /// Logs request details for debugging or monitoring purposes.
    /// </summary>
    /// <param name="request">The API Gateway request object.</param>
    /// <param name="context">The Lambda context.</param>
    private void LogRequestDetails(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context) {
        context.Logger.LogInformation("Processing request: {Path}, RequestId: {RequestId}", request.RawPath, context.AwsRequestId);

        foreach (KeyValuePair<string, string> header in request.Headers)
            context.Logger.LogDebug("Header: {Key} = {Value}", header.Key, header.Value);
    }
}
