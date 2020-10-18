using System.IO;
using MigrationTools.DataContracts;
using Newtonsoft.Json;

namespace MigrationTools.Endpoints
{
    public class FileSystemWorkItemEndpoint : WorkItemEndpoint
    {
        public FileSystemWorkItemEndpoint(FileSystemWorkItemEndpointOptions workItemEndpointOptions) : base(workItemEndpointOptions)
        {
        }

        public override void PersistWorkItem(WorkItemData source)
        {
            var content = JsonConvert.SerializeObject(source, Formatting.Indented);
            var fileName = Path.Combine(_WorkItemStoreQuery.Query, string.Format("{0}.json", source.Id));
            File.WriteAllText(fileName, content);
            RefreshStore();
        }
    }
}