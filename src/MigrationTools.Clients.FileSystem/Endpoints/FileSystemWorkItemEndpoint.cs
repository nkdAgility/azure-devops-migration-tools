using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using MigrationTools.DataContracts;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Options;
using Newtonsoft.Json;

namespace MigrationTools.Endpoints
{
    public class FileSystemWorkItemEndpoint : Endpoint, IWorkItemSourceEndpoint, IWorkItemTargetEndpoint
    {
        private FileSystemWorkItemEndpointOptions _options;
        private List<WorkItemData> _innerList;

        public FileSystemWorkItemEndpoint(EndpointEnricherContainer endpointEnrichers, ITelemetryLogger telemetry, ILogger<FileSystemWorkItemEndpoint> logger)
            : base(endpointEnrichers, telemetry, logger)
        {
            _innerList = new List<WorkItemData>();
        }

        public override int Count => GetWorkItems().Count();

        public override void Configure(IEndpointOptions options)
        {
            base.Configure(options);
            _options = (FileSystemWorkItemEndpointOptions)options;
            if (!Directory.Exists(_options.FileStore))
            {
                Directory.CreateDirectory(_options.FileStore);
            }
            LoadStore();
        }

        private void LoadStore()
        {
            _innerList.Clear();
            var workitemFiles = Directory.GetFiles(_options.FileStore);
            foreach (var item in workitemFiles)
            {
                var contents = File.ReadAllText(item);
                var workItem = JsonConvert.DeserializeObject<WorkItemData>(contents);
                _innerList.Add(workItem);
            }
        }

        public void Filter(IEnumerable<WorkItemData> workItems)
        {
            var ids = (from x in workItems.ToList() select x.Id);
            _innerList = (from x in _innerList
                          where !ids.Contains(x.Id)
                          select x).ToList();
        }

        public IEnumerable<WorkItemData> GetWorkItems()
        {
            return _innerList;
        }

        public IEnumerable<WorkItemData> GetWorkItems(QueryOptions query)
        {
            return GetWorkItems();
        }

        public void PersistWorkItem(WorkItemData source)
        {
            var content = JsonConvert.SerializeObject(source, Formatting.Indented);
            var fileName = Path.Combine(_options.FileStore, string.Format("{0}.json", source.Id));
            File.WriteAllText(fileName, content);
            LoadStore();
        }
    }
}