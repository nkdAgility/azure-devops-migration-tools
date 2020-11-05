namespace MigrationTools.Enrichers
{
    public interface IEndpointEnricher : IEnricher
    {
        void Configure(IEndpointEnricherOptions options);
    }
}