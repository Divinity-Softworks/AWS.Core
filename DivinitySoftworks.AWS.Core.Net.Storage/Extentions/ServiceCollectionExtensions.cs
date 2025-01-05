using Amazon;
using Amazon.S3;
using DivinitySoftworks.AWS.Core.Net.Storage;
using DivinitySoftworks.AWS.Core.Net.Storage.Settings;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for registering S3 bucket services in the DI container.
/// </summary>
internal static class ServiceCollectionExtensions {
    /// <summary>
    /// Adds the S3 bucket service to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The updated service collection.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the S3 bucket settings are missing.</exception>
    public static IServiceCollection AddS3Bucket(this IServiceCollection services, IConfiguration configuration) {
        // Retrieve S3 bucket settings from configuration
        S3BucketSettings? s3BucketSettings = configuration
            .GetSection(S3BucketSettings.KeyName)
            .Get<S3BucketSettings>()
            ?? throw new InvalidOperationException("S3 bucket settings are missing.");

        // Create the AmazonS3Client using the specified region
        AmazonS3Client amazonS3Client = new(s3BucketSettings.Region.ToRegionEndpoint());

        // Register the StorageService as a singleton
        services.AddSingleton<IStorageService>(new StorageService(amazonS3Client, s3BucketSettings.BucketName));

        return services;
    }
}
