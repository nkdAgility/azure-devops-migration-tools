namespace MigrationTools.Enrichers
{
    public interface IProcessorEnricher : IEnricher
    {
        void BeginProcessorExecution();
        void EndProcessorExecution();
    }
}