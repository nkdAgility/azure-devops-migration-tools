using MigrationTools.DataContracts;
using MigrationTools.Enrichers;

namespace MigrationTools.EndpointEnrichers
{
    public interface IEndpointEnricher : IEnricher
    {
        void Configure(IEndpointEnricherOptions options);
    }

    public interface IEndpointSourceEnricher : IEndpointEnricher
    {
    }

    public interface IWorkItemEndpointSourceEnricher : IEndpointSourceEnricher
    {
        void EnrichWorkItemData(Endpoints.IEndpoint endpoint, object dataSource, RevisionItem dataTarget);
    }

    public interface IEndpointTargetEnricher : IEndpointEnricher
    {
    }

    public interface IWorkItemEndpointTargetEnricher : IEndpointTargetEnricher
    {
    }
}