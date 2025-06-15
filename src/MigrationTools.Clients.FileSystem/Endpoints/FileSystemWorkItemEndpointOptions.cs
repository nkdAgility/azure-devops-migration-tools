using MigrationTools.Endpoints.Infrastructure;

namespace MigrationTools.Endpoints
{
    /// <summary>
    /// Configuration options for the File System Work Item Endpoint that stores work item data in local file system directories for offline processing or backup scenarios.
    /// </summary>
    public class FileSystemWorkItemEndpointOptions : EndpointOptions
    {
        /// <summary>
        /// Path to the directory where work item data will be stored or read from. This should be a valid local file system path with appropriate read/write permissions.
        /// </summary>
        public string FileStore { get; set; }

        //public override void SetDefaults()
        //{
        //    FileStore = @"c:\temp\Store";
        //}
    }
}