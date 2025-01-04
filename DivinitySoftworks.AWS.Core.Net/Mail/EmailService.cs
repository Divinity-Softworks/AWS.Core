using Amazon.Lambda.Core;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using DivinitySoftworks.Core.Net.Mail;

namespace DivinitySoftworks.AWS.Core.Net.Mail;

/// <summary>
/// Interface for sending emails using AWS services.
/// </summary>
public interface IEmailService {
    /// <summary>
    /// Sends an email message asynchronously using Amazon SES.
    /// </summary>
    /// <param name="emailMessage">The email message to send.</param>
    /// <param name="context">The AWS Lambda context for logging and execution details.</param>
    /// <returns>A <see cref="SendEmailResponse"/> object containing the response details, or null if an error occurs.</returns>
    Task<SendEmailResponse> SendAsync(EmailMessage emailMessage, ILambdaContext context);
    /// <summary>
    /// Sends an email template message asynchronously using Amazon SES.
    /// </summary>
    /// <param name="emailTemplateMessage">The email template message containing the template name, parameters, and other email details.</param>
    /// <param name="loadFileAsync">A function to asynchronously load email template files, taking the file path and AWS Lambda context as input.</param>
    /// <param name="context">The AWS Lambda context providing logging and execution details.</param>
    /// <returns>A <see cref="SendEmailResponse"/> object containing the response details, or null if sending fails.</returns>
    Task<SendEmailResponse?> SendAsync(EmailTemplateMessage emailTemplateMessage, Func<string, ILambdaContext, Task<string>> loadFileAsync, ILambdaContext context);
}

/// <summary>
/// Implementation of <see cref="IEmailService"/> for sending emails using AWS SES.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="EmailService"/> class.
/// </remarks>
/// <param name="amazonSimpleEmailService">The AWS Simple Email Service client.</param>
public sealed class EmailService(IAmazonSimpleEmailService amazonSimpleEmailService) : IEmailService, IDisposable {
    readonly IAmazonSimpleEmailService _amazonSimpleEmailService = amazonSimpleEmailService;
    bool _disposed;

    /// <inheritdoc/>
    public Task<SendEmailResponse> SendAsync(EmailMessage emailMessage, ILambdaContext context) {
        SendEmailRequest sendEmailRequest = new() {
            Source = emailMessage.Sender.ToString(),
            Destination = new Destination {
                ToAddresses = emailMessage.To,
                BccAddresses = emailMessage.BCC,
                CcAddresses = emailMessage.CC
            },
            Message = new Message {
                Subject = new Content(emailMessage.Subject),
                Body = new Body {
                    Html = new Content {
                        Charset = "UTF-8",
                        Data = emailMessage.HtmlBody
                    },
                    Text = new Content {
                        Charset = "UTF-8",
                        Data = emailMessage.TextBody
                    }
                }
            },
        };

        return _amazonSimpleEmailService.SendEmailAsync(sendEmailRequest);
    }

    /// <inheritdoc/>
    public async Task<SendEmailResponse?> SendAsync(EmailTemplateMessage emailTemplateMessage, Func<string, ILambdaContext, Task<string>> loadFileAsync, ILambdaContext context) {
        string htmlBody = await loadFileAsync($"{emailTemplateMessage.Template}.html", context);
        string textBody = await loadFileAsync($"{emailTemplateMessage.Template}.txt", context);


        context.Logger.LogInformation($"Found [{emailTemplateMessage.Parameters.Count}] parameters!");
        foreach (KeyValuePair<string, string> parameter in emailTemplateMessage.Parameters) {
            context.Logger.LogInformation($"Parameter [{parameter.Key}], Value [{parameter.Value}]!");
            htmlBody = htmlBody.Replace("$[" + parameter.Key + "]", parameter.Value);
            textBody = textBody.Replace("$[" + parameter.Key + "]", parameter.Value);
        }

        EmailMessage emailMessage = new(emailTemplateMessage.Sender, emailTemplateMessage.Subject ?? string.Empty) {
            Attachments = emailTemplateMessage.Attachments,
            BCC = emailTemplateMessage.BCC,
            CC = emailTemplateMessage.CC,
            Headers = emailTemplateMessage.Headers,
            HtmlBody = htmlBody,
            Priority = emailTemplateMessage.Priority,
            ReplyTo = emailTemplateMessage.ReplyTo,
            SentDate = emailTemplateMessage.SentDate,
            TextBody = textBody,
            To = emailTemplateMessage.To
        };

        return await SendAsync(emailMessage, context);
    }

    /// <summary>
    /// Releases the resources used by the EmailService.
    /// </summary>
    private void Dispose(bool disposing) {
        if (!_disposed) {
            if (disposing)
                _amazonSimpleEmailService?.Dispose();

            _disposed = true;
        }
    }

    /// <inheritdoc />
    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
