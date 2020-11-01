using MigrationTools.Options;

namespace MigrationTools.Endpoints
{
    public class TfsWorkItemEndPointOptions : EndpointOptions
    {
        public override string Endpoint => nameof(TfsWorkItemEndPoint);

        private QueryOptions Query { get; set; }

        public override void SetDefaults()
        {
            Direction = EndpointDirection.NotSet;
        }
    }
}