using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using MigrationTools.DataContracts;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Enrichers;
using MigrationTools.Options;

namespace MigrationTools.Endpoints
{
    public class InMemoryWorkItemEndpoint : Endpoint, IWorkItemSourceEndpoint, IWorkItemTargetEndpoint
    {
        private List<WorkItemData> _innerList;
        private InMemoryWorkItemEndpointOptions _Options;

        public InMemoryWorkItemEndpoint(EndpointEnricherContainer endpointEnrichers, ITelemetryLogger telemetry, ILogger<InMemoryWorkItemEndpoint> logger)
            : base(endpointEnrichers, telemetry, logger)
        {
            _innerList = new List<WorkItemData>();
        }

        public override int Count => _innerList.Count;

        public override void Configure(IEndpointOptions options)
        {
            base.Configure(options);
            _Options = (InMemoryWorkItemEndpointOptions)options;
        }

        public WorkItemData CreateNewFrom(WorkItemData source)
        {
            _innerList.Add(source);
            return source;
        }

        public void Filter(IEnumerable<WorkItemData> workItems)
        {
            var ids = (from x in workItems select x.Id);
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
            var found = (from x in _innerList where x.Id == source.Id select x).SingleOrDefault();
            if (found is null)
            {
                found = CreateNewFrom(source);
            }
            foreach (IWorkItemProcessorTargetEnricher enricher in TargetEnrichers)
            {
                enricher.PersistFromWorkItem(source);
            }
            UpdateWorkItemFrom(found, source);
        }

        private void UpdateWorkItemFrom(WorkItemData source, WorkItemData target)
        {
            _innerList.Remove(source);
            _innerList.Add(target);
        }
    }
}