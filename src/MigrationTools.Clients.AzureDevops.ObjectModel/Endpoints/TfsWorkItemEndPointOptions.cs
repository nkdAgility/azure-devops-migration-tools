using System;
using MigrationTools.Options;

namespace MigrationTools.Endpoints
{
    public class TfsWorkItemEndpointOptions : TfsEndpointOptions
    {
        public override Type ToConfigure => typeof(TfsWorkItemEndpoint);

        public QueryOptions Query { get; set; }

        public override void SetDefaults()
        {
            Direction = EndpointDirection.NotSet;
        }
    }
}