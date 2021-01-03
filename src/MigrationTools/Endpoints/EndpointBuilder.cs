using Microsoft.Extensions.DependencyInjection;

namespace MigrationTools.Endpoints
{
    public class EndpointBuilder : IEndpointBuilder
    {
        public EndpointBuilder(IServiceCollection services, string name)
        {
            Services = services;
            Name = name;
        }

        public string Name { get; }
        public IServiceCollection Services { get; }
    }
}
