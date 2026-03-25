using Hammer.Support.Application.Abstractions;
using Hammer.Support.Infrastructure.Kafka;
using Hammer.Support.Infrastructure.Onbid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hammer.Support.Infrastructure;

/// <summary>
/// Registers infrastructure services into the DI container.
/// </summary>
public static class InfrastructureServiceRegistration
{
    /// <summary>
    /// Adds infrastructure services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        // Kafka
        services.Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.SectionName));
        services.AddSingleton<IEventPublisher, KafkaEventPublisher>();

        // Onbid
        services.Configure<OnbidOptions>(configuration.GetSection(OnbidOptions.SectionName));
        services.AddHttpClient<IKamcoApiClient, KamcoApiClient>();
        services.AddScoped<ICollectKamcoAuctionsUseCase, CollectKamcoAuctionsUseCase>();
        services.AddHostedService<KamcoCollectionJob>();

        return services;
    }
}
