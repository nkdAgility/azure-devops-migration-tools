using System;
using MigrationTools.Options;

namespace MigrationTools.Endpoints
{
    public class TfsWorkItemEndPointOptions : EndpointOptions
    {
        public override Type ToConfigure => typeof(TfsWorkItemEndPoint);

        public QueryOptions Query { get; set; }

        public string AccessToken { get; set; }
        public string Organisation { get; set; }
        public string Project { get; set; }

        public override void SetDefaults()
        {
            Direction = EndpointDirection.NotSet;
        }
    }
}