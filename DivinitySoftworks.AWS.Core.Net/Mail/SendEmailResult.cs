using Amazon.SimpleEmail.Model;

namespace DivinitySoftworks.AWS.Core.Net.Mail;
/// <summary>
/// Represents the result of a send email operation, extending the AWS SendEmailResponse
/// with additional information such as errors encountered during processing.
/// </summary>
public sealed class SendEmailResult : SendEmailResponse {
    /// <summary>
    /// Gets or sets a list of error messages encountered during the send email operation.
    /// </summary>
    public List<string> Errors { get; set; } = [];

    /// <summary>
    /// Maps a <see cref="SendEmailResponse"/> to a <see cref="SendEmailResult"/>.
    /// </summary>
    public static SendEmailResult FromResponse(SendEmailResponse response) {
        return new SendEmailResult {
            MessageId = response.MessageId,
            HttpStatusCode = response.HttpStatusCode,
            ResponseMetadata = response.ResponseMetadata,
            ContentLength = response.ContentLength
        };
    }
}