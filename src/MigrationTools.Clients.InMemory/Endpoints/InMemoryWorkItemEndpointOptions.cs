using System;

namespace MigrationTools.Endpoints
{
    public class InMemoryWorkItemEndpointOptions : EndpointOptions, IWorkItemEndpoint
    {
        public override Type ToConfigure => typeof(InMemoryWorkItemEndpoint);

        public override void SetDefaults()
        {
        }
    }
}