using Amazon;
using Amazon.SimpleEmail;
using DivinitySoftworks.AWS.Core.Net.Mail;
using DivinitySoftworks.AWS.Core.Net.Settings;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for configuring AWS Simple Email Service (SES) in an <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionExtensions {
    /// <summary>
    /// Adds and configures the AWS Simple Email Service (SES) client to the dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="configuration">The application's configuration from which the SES settings will be retrieved.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the SES settings are missing or invalid in the configuration.</exception>
    public static IServiceCollection AddSimpleEmailService(this IServiceCollection services, IConfiguration configuration) {
        SimpleEmailServiceSettings? simpleEmailServiceSettings = configuration
            .GetSection(SimpleEmailServiceSettings.KeyName)
            .Get<SimpleEmailServiceSettings>()
            ?? throw new InvalidOperationException("Simple Email Service settings are missing.");

        IAmazonSimpleEmailService amazonSimpleEmailService = new AmazonSimpleEmailServiceClient(simpleEmailServiceSettings.Region.ToRegionEndpoint());

        services.AddSingleton<IEmailService>(new EmailService(amazonSimpleEmailService));

        return services;
    }
}
