using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DivinitySoftworks.AWS.Core.Web.Functions.Contracts;

/// <summary>
/// Represents the response returned by the health check function.
/// </summary>
public record HealthCheckResponse {
    /// <summary>
    /// Gets the health status of the system or component.
    /// </summary>
    /// <remarks>
    /// This property provides a read-only representation of the current health status,
    /// which is defined by the <see cref="HealthStatus"/> enumeration.
    /// The possible values are:
    /// <list type="bullet">
    ///     <item><description><see cref="HealthStatus.Healthy"/> - The system or component is healthy and operational.</description></item>
    ///     <item><description><see cref="HealthStatus.Degraded"/> - The system or component is degraded but still operational.</description></item>
    ///     <item><description><see cref="HealthStatus.Unhealthy"/> - The system or component is unhealthy and not operational.</description></item>
    /// </list>
    /// </remarks>
    public HealthStatus Status { get; init; }
}

