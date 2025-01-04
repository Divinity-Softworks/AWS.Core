using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using System.Collections.Concurrent;

namespace DivinitySoftworks.AWS.Core.Net.Storage; 

/// <summary>
/// Defines a contract for a storage service that can load files from an S3 bucket.
/// </summary>
public interface IStorageService {
    /// <summary>
    /// Loads a file from the S3 bucket and caches it for future use.
    /// </summary>
    /// <param name="key">The S3 object key (path to the file).</param>
    /// <param name="context">The AWS Lambda context for logging and execution details.</param>
    /// <returns>The file content as a string.</returns>
    Task<string> LoadFileAsync(string key, ILambdaContext context);
}

/// <summary>
/// A service that provides access to files stored in an S3 bucket, with caching to reduce repeated requests.
/// </summary>
public sealed class StorageService(AmazonS3Client amazonS3Client, string bucketName) : IStorageService, IDisposable {
    private readonly AmazonS3Client _amazonS3Client = amazonS3Client;
    private readonly string _bucketName = bucketName;
    private bool _disposed;

    /// <summary>
    /// A thread-safe cache for storing file content to minimize S3 calls.
    /// </summary>
    private readonly ConcurrentDictionary<string, string> _templates = new();

    /// <inheritdoc />
    public async Task<string> LoadFileAsync(string key, ILambdaContext? context) {
        try {
            if (_templates.TryGetValue(key, out string? value))
                return value;

            GetObjectRequest request = new() {
                BucketName = _bucketName,
                Key = key
            };

            using GetObjectResponse getObjectResponse = await _amazonS3Client.GetObjectAsync(request);
            using StreamReader streamReader = new(getObjectResponse.ResponseStream);

            string content = await streamReader.ReadToEndAsync();
            _templates.TryAdd(key, content);

            return content;
        }
        catch (AmazonS3Exception exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound) {
            context?.Logger.LogWarning($"S3 file not found: {key}");
            return string.Empty;
        }
        catch (Exception exception) {
            context?.Logger.LogError($"Error retrieving S3 file {key}: {exception.Message}");
            return string.Empty;
        }
    }

    /// <summary>
    /// Releases the resources used by the StorageService.
    /// </summary>
    /// <param name="disposing">Indicates whether managed resources should be released.</param>
    private void Dispose(bool disposing) {
        if (!_disposed) {
            if (disposing) 
                _amazonS3Client?.Dispose();
            _disposed = true;
        }
    }

    /// <inheritdoc />
    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
