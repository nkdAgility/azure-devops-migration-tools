using Microsoft.Extensions.DependencyInjection;

namespace MigrationTools.Endpoints
{
    public interface IEndpointBuilder
    {
        string Name { get; }
        IServiceCollection Services { get; }

    }
}
