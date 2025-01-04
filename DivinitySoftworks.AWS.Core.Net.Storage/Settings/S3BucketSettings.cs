namespace DivinitySoftworks.AWS.Core.Net.Storage.Settings;

/// <summary>
/// Represents the settings required for configuring an Amazon S3 Bucket.
/// </summary>
internal class S3BucketSettings {
    /// <summary>
    /// The configuration key name for AWS S3 settings.
    /// </summary>
    public const string KeyName = "AWS.S3";

    /// <summary>
    /// Gets or sets the name of the S3 bucket.
    /// </summary>
    public string BucketName { get; set; } = default!;

    /// <summary>
    /// The AWS region where the S3 bucket is hosted.
    /// </summary>
    public string Region { get; set; } = default!;
}
