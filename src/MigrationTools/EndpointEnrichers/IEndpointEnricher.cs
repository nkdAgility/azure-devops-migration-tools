using MigrationTools.DataContracts;
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
        void EnrichWorkItemData(Endpoints.IEndpoint endpoint, object dataSource, RevisionItem dataTarget);
    }

    public interface IWorkItemEndpointTargetEnricher : IWorkItemEndpoinEnricher
    {
    }
}