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
    /// Sends an email using Amazon Simple Email Service (SES) with optional email address filtering and validation.
    /// </summary>
    /// <param name="emailMessage">
    /// The <see cref="EmailMessage"/> object containing the details of the email to be sent, 
    /// including sender, recipients, subject, and message body (HTML or plain text).
    /// </param>
    /// <param name="checkEmailAddresses">
    /// An optional function that validates and filters recipient email addresses. 
    /// This function takes a list of email addresses and a <see cref="ILambdaContext"/>, 
    /// and returns a filtered list of valid email addresses.
    /// </param>
    /// <param name="context">
    /// The <see cref="ILambdaContext"/> instance providing context information about the AWS Lambda execution environment.
    /// </param>
    /// <returns>
    /// A <see cref="SendEmailResult"/> representing the result of the send email operation, 
    /// including success status, metadata, and any errors encountered.
    /// </returns>
    /// <remarks>
    /// - Ensures the email message contains either a text or HTML body.
    /// - Optionally filters recipient email addresses using the provided <paramref name="checkEmailAddresses"/> function.
    /// - Validates that there is at least one recipient (To, CC, or BCC) after filtering.
    /// - Catches exceptions during the send operation and returns an error result.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown if the <paramref name="emailMessage"/> parameter is null.
    /// </exception>
    Task<SendEmailResult> SendAsync(EmailMessage emailMessage, Func<List<string>, ILambdaContext, Task<List<string>>>? checkEmailAddresses, ILambdaContext context);
    /// <summary>
    /// Sends an email using a predefined template, with optional email address filtering and parameter substitution.
    /// </summary>
    /// <param name="emailTemplateMessage">
    /// The <see cref="EmailTemplateMessage"/> object containing the details of the email to be sent, 
    /// including sender, recipients, template name, parameters for substitution, and optional attachments.
    /// </param>
    /// <param name="loadFileAsync">
    /// A function to asynchronously load the HTML and plain text templates from a file or storage. 
    /// The function takes the template file path and an <see cref="ILambdaContext"/>, 
    /// and returns the file content as a string.
    /// </param>
    /// <param name="checkEmailAddresses">
    /// An optional function that validates and filters recipient email addresses. 
    /// This function takes a list of email addresses and a <see cref="ILambdaContext"/>, 
    /// and returns a filtered list of valid email addresses.
    /// </param>
    /// <param name="context">
    /// The <see cref="ILambdaContext"/> instance providing context information about the AWS Lambda execution environment.
    /// </param>
    /// <returns>
    /// A <see cref="SendEmailResult"/> representing the result of the send email operation, 
    /// including success status, metadata, and any errors encountered.
    /// </returns>
    /// <remarks>
    /// - Loads the email template files for HTML and plain text content using the <paramref name="loadFileAsync"/> function.
    /// - Substitutes template parameters with values provided in <see cref="EmailTemplateMessage.Parameters"/>.
    /// - Validates and filters recipient email addresses using the optional <paramref name="checkEmailAddresses"/> function.
    /// - Sends the email using the <see cref="SendAsync(EmailMessage, Func{List{string}, ILambdaContext, Task{List{string}}}?, ILambdaContext)"/> method.
    /// </remarks>
    Task<SendEmailResult> SendAsync(EmailTemplateMessage emailTemplateMessage, Func<string, ILambdaContext, Task<string>> loadFileAsync, Func<List<string>, ILambdaContext, Task<List<string>>>? checkEmailAddresses, ILambdaContext context);
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
    public async Task<SendEmailResult> SendAsync(EmailMessage emailMessage, Func<List<string>, ILambdaContext, Task<List<string>>>? checkEmailAddresses, ILambdaContext context) {
        // Make sure the 'emailMessage' has an 'Html' and/or 'Text' body.
        if (string.IsNullOrWhiteSpace(emailMessage.TextBody) && string.IsNullOrWhiteSpace(emailMessage.HtmlBody))
            return new SendEmailResult() {
                ContentLength = 0,
                HttpStatusCode = System.Net.HttpStatusCode.BadRequest,
                Errors = ["Email body (Text or HTML) must not be empty."]
            };

        // Check email addresses if required, this should check a blacklist to see if an email adress should be removed from the list(s).
        if (checkEmailAddresses is not null) {
            emailMessage.To = await checkEmailAddresses(emailMessage.To, context);
            emailMessage.CC = await checkEmailAddresses(emailMessage.CC, context);
            emailMessage.BCC = await checkEmailAddresses(emailMessage.BCC, context);
        }

        // Make sure whe have at least one email adress to send an email to.
        if (emailMessage.To.Count == 0 && emailMessage.CC.Count == 0 && emailMessage.BCC.Count == 0)
            return new SendEmailResult() {
                ContentLength = 0,
                HttpStatusCode = System.Net.HttpStatusCode.BadRequest,
                Errors = ["No email address found to send an email to."]
            };

        try {
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

            return SendEmailResult.FromResponse(await _amazonSimpleEmailService.SendEmailAsync(sendEmailRequest));
        }
        catch (Exception exception) {
            return new SendEmailResult {
                HttpStatusCode = System.Net.HttpStatusCode.InternalServerError,
                Errors = [exception.Message]
            };
        }
    }

    /// <inheritdoc/>
    public async Task<SendEmailResult> SendAsync(EmailTemplateMessage emailTemplateMessage, Func<string, ILambdaContext, Task<string>> loadFileAsync, Func<List<string>, ILambdaContext, Task<List<string>>>? checkEmailAddresses, ILambdaContext context) {
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

        return await SendAsync(emailMessage, checkEmailAddresses, context);
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
