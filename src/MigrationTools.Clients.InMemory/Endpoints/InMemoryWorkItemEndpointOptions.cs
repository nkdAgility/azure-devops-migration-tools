using System;

namespace MigrationTools.Endpoints
{
    public class InMemoryWorkItemEndpointOptions : EndpointOptions
    {
        public override Type ToConfigure => typeof(InMemoryWorkItemEndpoint);

        public void Configure(IEndpointOptions options)
        {
            throw new NotImplementedException();
        }

        public override void SetDefaults()
        {
        }
    }
}