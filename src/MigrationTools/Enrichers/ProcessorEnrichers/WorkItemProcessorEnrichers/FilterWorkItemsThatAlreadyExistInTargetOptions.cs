using MigrationTools.Options;

namespace MigrationTools.Enrichers
{
    internal class FilterWorkItemsThatAlreadyExistInTargetOptions : ProcessorEnricherOptions
    {
        public QueryOptions Query { get; set; }

        public override void SetDefaults()
        {
            Enabled = true;
            Query = new QueryOptions() { WhereBit = "AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')", OrderBit = "[System.ChangedDate] desc" };
        }
    }
}