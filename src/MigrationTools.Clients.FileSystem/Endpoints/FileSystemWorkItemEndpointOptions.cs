using System;

namespace MigrationTools.Endpoints
{
    public class FileSystemWorkItemEndpointOptions : EndpointOptions
    {
        public override Type ToConfigure => typeof(FileSystemWorkItemEndpoint);

        public override void SetDefaults()
        {
            Direction = EndpointDirection.NotSet;
        }
    }
}