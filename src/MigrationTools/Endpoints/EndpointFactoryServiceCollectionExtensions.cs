using System;
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
