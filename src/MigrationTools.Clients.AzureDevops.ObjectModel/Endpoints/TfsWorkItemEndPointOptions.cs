using System;
using MigrationTools.Options;

namespace MigrationTools.Endpoints
{
    public class TfsWorkItemEndpointOptions : TfsEndpointOptions, ITfsWorkItemEndpointOptions
    {
        public override Type ToConfigure => typeof(TfsWorkItemEndpoint);

        public QueryOptions Query { get; set; }

        public override void SetDefaults()
        {
            base.SetDefaults();
            Direction = EndpointDirection.NotSet;
        }
    }

    public interface ITfsWorkItemEndpointOptions
    {
        public QueryOptions Query { get; }
    }
}