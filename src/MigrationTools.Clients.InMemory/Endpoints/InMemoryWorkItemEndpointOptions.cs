using System;
using MigrationTools.EndpointEnrichers;

namespace MigrationTools.Endpoints
{
    public class InMemoryWorkItemEndpointOptions : EndpointOptions, IWorkItemEndpoint
    {
        public override Type ToConfigure => typeof(InMemoryWorkItemEndpoint);

        EndpointEnricherContainer IEndpoint.EndpointEnrichers => throw new NotImplementedException();

        public void Configure(IEndpointOptions options)
        {
            throw new NotImplementedException();
        }

        public override void SetDefaults()
        {
        }
    }
}