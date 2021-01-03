using System;
using System.Collections.Generic;
using MigrationTools.Options;

namespace MigrationTools.Endpoints
{
    public class TfsWorkItemEndpointOptions : TfsEndpointOptions
    {
        public override Type ToConfigure => typeof(TfsWorkItemEndpoint);

        public QueryOptions Query { get; set; }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Query = new Options.QueryOptions()
            {
                Query = "SELECT [System.Id], [System.Tags] " +
                             "FROM WorkItems " +
                             "WHERE [System.TeamProject] = @TeamProject " +
                                 "AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan') " +
                             "ORDER BY [System.ChangedDate] desc",
                Paramiters = new Dictionary<string, string>() { { "TeamProject", "migrationSource1" } }
            };
        }
    }

    public interface ITfsWorkItemEndpointOptions
    {
        public QueryOptions Query { get; }
    }
}