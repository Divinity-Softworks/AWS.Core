namespace DivinitySoftworks.AWS.Core.Net.Settings;

/// <summary>
/// Represents the settings required to configure the Simple Email Service (SES).
/// </summary>
public sealed record SimpleEmailServiceSettings {
    /// <summary>
    /// The key name for the simple email service settings.
    /// </summary>
    public const string KeyName = "AWS.SES";

    /// <summary>
    /// The AWS region where the SES service is hosted.
    /// </summary>
    public string Region { get; set; } = default!;
}