using System.IO;
using Microsoft.Extensions.Logging;
using MigrationTools.DataContracts;
using Newtonsoft.Json;

namespace MigrationTools.Endpoints
{
    public class FileSystemWorkItemEndpoint : WorkItemEndpoint
    {
        public FileSystemWorkItemEndpoint(System.IServiceProvider services, ITelemetryLogger telemetry, ILogger<WorkItemEndpoint> logger) : base(services, telemetry, logger)
        {
        }

        public override void PersistWorkItem(WorkItemData2 source)
        {
            var content = JsonConvert.SerializeObject(source, Formatting.Indented);
            var fileName = Path.Combine(_WorkItemStoreQuery.Query, string.Format("{0}.json", source.Id));
            File.WriteAllText(fileName, content);
            RefreshStore();
        }
    }
}