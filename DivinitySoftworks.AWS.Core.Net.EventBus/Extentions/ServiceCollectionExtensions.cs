using Amazon;
using Amazon.SimpleNotificationService;
using DivinitySoftworks.AWS.Core.Net.EventBus.Settings;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for configuring AWS Simple Notification Service (SNS) in an <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionExtensions {
    /// <summary>
    /// Adds and configures the AWS Simple Notification Service (SNS) client to the dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="configuration">The application's configuration from which the SNS settings will be retrieved.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the SNS settings are missing or invalid in the configuration.</exception>
    public static IServiceCollection AddSimpleNotificationService(this IServiceCollection services, IConfiguration configuration) {
        SimpleNotificationServiceSettings? simpleNotificationServiceSettings = configuration
            .GetSection(SimpleNotificationServiceSettings.KeyName)
            .Get<SimpleNotificationServiceSettings>()
            ?? throw new InvalidOperationException("Simple Notification Service settings are missing.");

        services.AddSingleton<IAmazonSimpleNotificationService>(
            new AmazonSimpleNotificationServiceClient(simpleNotificationServiceSettings.Region.ToRegionEndpoint()));

        return services;
    }
}
