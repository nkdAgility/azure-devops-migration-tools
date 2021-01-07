using System;

namespace MigrationTools.Endpoints
{
    public class FileSystemWorkItemEndpointOptions : EndpointOptions
    {
        public string FileStore { get; set; }

        //public override void SetDefaults()
        //{
        //    FileStore = @"c:\temp\Store";
        //}
    }
}