using System;
using System.Collections.Generic;
using MigrationTools.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MigrationTools.Endpoints
{
    public enum AuthenticationMode
    {
        AccessToken = 0,
        Windows = 1,
        Prompt = 2
    }

    public class TfsWorkItemEndPointOptions : EndpointOptions
    {
        public override Type ToConfigure => typeof(TfsWorkItemEndpoint);

        public QueryOptions Query { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public AuthenticationMode AuthenticationMode { get; set; }

        public string AccessToken { get; set; }
        public string Organisation { get; set; }
        public string Project { get; set; }

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