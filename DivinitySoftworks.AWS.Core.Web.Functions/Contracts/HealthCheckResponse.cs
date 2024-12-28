using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DivinitySoftworks.Core.Web.Functions.Contracts;

/// <summary>
/// Represents the response returned by the health check function.
/// </summary>
public record HealthCheckResponse {
    /// <summary>
    /// Gets or sets the status of the health check.
    /// </summary>
    public HealthStatus Status { get; init; }
}