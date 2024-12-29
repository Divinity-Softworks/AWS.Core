using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using DivinitySoftworks.Core.Net.EventBus;
using System.Text.Json;

namespace DivinitySoftworks.AWS.Core.Net.EventBus;

/// <summary>
/// Implements the <see cref="IPublisher"/> interface to publish messages to AWS Simple Notification Service (SNS).
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="PublisherService"/> class.
/// </remarks>
/// <param name="amazonSimpleNotificationService">The <see cref="IAmazonSimpleNotificationService"/> client for interacting with SNS.</param>
public sealed class PublisherService(IAmazonSimpleNotificationService amazonSimpleNotificationService) : IPublisher {
    private readonly IAmazonSimpleNotificationService _amazonSimpleNotificationService = amazonSimpleNotificationService;

    /// <inheritdoc/>
    /// <summary>
    /// Publishes a message to the specified SNS topic (bus) and returns a result asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the message to publish.</typeparam>
    /// <typeparam name="R">The type of the result returned after publishing.</typeparam>
    /// <param name="busName">The ARN (Amazon Resource Name) of the SNS topic to publish the message to.</param>
    /// <param name="message">The message to publish to the SNS topic.</param>
    /// <returns>
    /// A task representing the asynchronous operation, containing the result of type <typeparamref name="R"/>.
    /// </returns>
    /// <exception cref="AmazonSimpleNotificationServiceException">
    /// Thrown when there is an error with the SNS service while publishing the message.
    /// </exception>
    public async Task<R?> PublishAsync<T, R>(string busName, T message) {
        PublishRequest request = new() {
            TopicArn = busName,
            Message = JsonSerializer.Serialize(message)
        };

        // Publish the message to SNS and await the response
        PublishResponse publishResponse = await _amazonSimpleNotificationService.PublishAsync(request);

        // Deserialize the response into the expected result type
        return JsonSerializer.Deserialize<R>(JsonSerializer.Serialize(publishResponse));
    }
}
