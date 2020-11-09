using MigrationTools.Enrichers;

namespace MigrationTools.EndpointEnrichers
{
    public interface IEndpointEnricher : IEnricher
    {
        void Configure(IEndpointEnricherOptions options);
    }

    public interface IWorkItemEndpoinEnricher : IEndpointEnricher
    {
    }

    public interface IWorkItemEndpointSourceEnricher : IWorkItemEndpoinEnricher
    {
    }

    public interface IWorkItemEndpointTargetEnricher : IWorkItemEndpoinEnricher
    {
    }
}