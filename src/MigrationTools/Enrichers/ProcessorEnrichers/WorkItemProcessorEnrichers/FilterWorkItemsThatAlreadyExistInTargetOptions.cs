using MigrationTools.Options;

namespace MigrationTools.Enrichers
{
    internal class FilterWorkItemsThatAlreadyExistInTargetOptions : ProcessorEnricherOptions
    {
        public QueryOptions Query { get; set; }

        public override void SetDefaults()
        {
            Enabled = true;
            Query = new QueryOptions() { Query = "SELECT [System.Id], [System.Tags] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan') ORDER BY [System.ChangedDate] desc" };
        }
    }
}