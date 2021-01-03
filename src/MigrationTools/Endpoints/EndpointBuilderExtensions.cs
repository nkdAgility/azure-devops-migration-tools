using System;
using Microsoft.Extensions.DependencyInjection;

namespace MigrationTools.Endpoints
{
    public static class EndpointBuilderExtensions
    {
        public static IEndpointBuilder ConfigureEndpoint(this IEndpointBuilder builder, Func<IServiceProvider, IEndpoint> createEndpoint)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (createEndpoint == null)
            {
                throw new ArgumentNullException(nameof(createEndpoint));
            }

            builder.Services.Configure<EndpointFactoryOptions>(builder.Name, options => options.EndpointFuncs.Add(createEndpoint));

            return builder;
        }
    }
}
