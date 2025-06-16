using MigrationTools.Options;

namespace MigrationTools.Endpoints
{
    /// <summary>
    /// Configuration options for the TFS/Azure DevOps Work Item Endpoint that defines connection settings and query options for accessing work items in Team Foundation Server or Azure DevOps Server.
    /// </summary>
    public class TfsWorkItemEndpointOptions : TfsEndpointOptions
    {
        /// <summary>
        /// Gets or sets the query options that define which work items to retrieve from the source endpoint, including WIQL queries and parameters.
        /// </summary>
        public QueryOptions Query { get; set; }

        //public override void SetDefaults()
        //{
        //    base.SetDefaults();
        //    Query = new Options.QueryOptions()
        //    {
        //        Query = "SELECT [System.Id], [System.Tags] " +
        //                     "FROM WorkItems " +
        //                     "WHERE [System.TeamProject] = @TeamProject " +
        //                         "AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan') " +
        //                     "ORDER BY [System.ChangedDate] desc",
        //        Paramiters = new Dictionary<string, string>() { { "TeamProject", "migrationSource1" } }
        //    };
        //}
    }
}