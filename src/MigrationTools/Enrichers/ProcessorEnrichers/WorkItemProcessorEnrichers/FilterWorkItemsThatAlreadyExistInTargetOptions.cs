using MigrationTools.Options;

namespace MigrationTools.Enrichers
{
    internal class FilterWorkItemsThatAlreadyExistInTargetOptions : ProcessorEnricherOptions
    {
        public QueryOptions Query { get; set; }
    }
}