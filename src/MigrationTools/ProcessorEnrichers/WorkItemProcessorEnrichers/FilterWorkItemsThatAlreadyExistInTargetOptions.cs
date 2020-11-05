using System;
using MigrationTools.Options;

namespace MigrationTools.Enrichers
{
    public class FilterWorkItemsThatAlreadyExistInTargetOptions : ProcessorEnricherOptions
    {
        public QueryOptions Query { get; set; }

        public override Type ToConfigure => typeof(FilterWorkItemsThatAlreadyExistInTarget);

        public override void SetDefaults()
        {
            Enabled = true;
            Query = new QueryOptions() { Query = "SELECT [System.Id], [System.Tags] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan') ORDER BY [System.ChangedDate] desc" };
        }
    }
}