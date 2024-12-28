using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace Amazon.Lambda.APIGatewayEvents;

/// <summary>
/// Provides extension methods for the <see cref="APIGatewayHttpApiV2ProxyRequest"/> class.
/// </summary>
public static class APIGatewayHttpApiV2ProxyRequestExtentions {

    /// <summary>
    /// Checks if the body of the <paramref name="request"/> is encoded as Base64. Decodes it, and converts the value to a <seealso cref="NameValueCollection"/>. It will also URL decode all the values from the collection.
    /// </summary>
    /// <param name="request">The request to get the body parameters from.</param>
    /// <returns>All parameters from the <paramref name="request"/> body as a <seealso cref="NameValueCollection"/>.</returns>
    public static NameValueCollection ToNameValueCollection(this APIGatewayHttpApiV2ProxyRequest request) {
        ArgumentNullException.ThrowIfNull(request);

        string body = (request.IsBase64Encoded) ? new UTF8Encoding().GetString(Convert.FromBase64String(request.Body)) : request.Body;

        // Parse the Body Parameters
        return HttpUtility.ParseQueryString(body);
    }
}
