using System;
using MigrationTools.Options;

namespace MigrationTools.Endpoints
{
    public class TfsWorkItemEndPointOptions : EndpointOptions
    {
        public override Type ToConfigure => typeof(TfsWorkItemEndPoint);

        private QueryOptions Query { get; set; }

        private string AccessToken { get; set; }

        public override void SetDefaults()
        {
            Direction = EndpointDirection.NotSet;
        }
    }
}