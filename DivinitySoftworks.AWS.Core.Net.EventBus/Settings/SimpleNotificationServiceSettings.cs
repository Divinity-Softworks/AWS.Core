namespace DivinitySoftworks.AWS.Core.Net.EventBus.Settings; 

/// <summary>
/// Represents the settings required to configure the Simple Notification Service (SNS).
/// </summary>
public sealed record SimpleNotificationServiceSettings {
    /// <summary>
    /// The key name for the simple notification service settings.
    /// </summary>
    public const string KeyName = "AWS.SNS";

    /// <summary>
    /// The AWS region where the SNS service is hosted.
    /// </summary>
    public string Region { get; set; } = default!;
}