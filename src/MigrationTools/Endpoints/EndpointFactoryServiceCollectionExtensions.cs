using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MigrationTools.Endpoints
{
    public static class EndpointFactoryServiceCollectionExtensions
    {
        public static IServiceCollection AddEndpoint(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddLogging();
            services.AddOptions();

            services.TryAddSingleton<EndpointFactory>();
            services.TryAddSingleton<IEndpointFactory>(serviceProvider => serviceProvider.GetRequiredService<EndpointFactory>());

            return services;
        }

        public static IServiceCollection AddEndPoints<TOptions, TEndpoint>(this IServiceCollection services, IConfiguration configuration, string settingsName)
            where TOptions : EndpointOptions
            where TEndpoint : Endpoint
        {
            services.AddTransient<TEndpoint>();

            var endPoints = configuration.GetSection($"Endpoints:{settingsName}");
            var children = endPoints.GetChildren();
            foreach (var child in children)
            {
                var options = child.Get<TOptions>();
                services.AddEndpoint(options.Name, (provider) =>
                {
                    var endpoint = provider.GetRequiredService<TEndpoint>();
                    endpoint.Configure(options);
                    return endpoint;
                });
            }
            return services;
        }

        public static IEndpointBuilder AddEndpoint(this IServiceCollection services, string name, Func<IServiceProvider, IEndpoint> createEndpoint)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (createEndpoint == null)
            {
                throw new ArgumentNullException(nameof(createEndpoint));
            }

            AddEndpoint(services);

            var builder = new EndpointBuilder(services, name);
            builder.ConfigureEndpoint(createEndpoint);
            return builder;
        }
    }
}
