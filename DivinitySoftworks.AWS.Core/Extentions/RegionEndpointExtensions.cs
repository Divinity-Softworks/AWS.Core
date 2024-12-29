namespace Amazon;

/// <summary>
/// Provides extension methods for working with AWS RegionEndpoint objects.
/// </summary>
public static class RegionEndpointExtensions {
    /// <summary>
    /// Converts a string representing a region (e.g., "us-east-1") to a <see cref="RegionEndpoint"/> object.
    /// </summary>
    /// <param name="region">The string representation of the AWS region.</param>
    /// <returns>The corresponding <see cref="RegionEndpoint"/> object.</returns>
    /// <exception cref="ArgumentException">Thrown when the region string is null, empty, or does not match any known AWS region.</exception>
    public static RegionEndpoint ToRegionEndpoint(this string region) {
        if (string.IsNullOrWhiteSpace(region))
            throw new ArgumentException("The region string cannot be null or empty.", nameof(region));

        // Loop through all RegionEndpoint properties to find a match
        foreach (RegionEndpoint endpoint in RegionEndpoint.EnumerableAllRegions) {
            if (string.Equals(endpoint.SystemName, region, StringComparison.OrdinalIgnoreCase)) 
                return endpoint;
        }

        // If no match is found, throw an exception
        throw new ArgumentException($"The region '{region}' is not a valid AWS region.", nameof(region));
    }
}