using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;

namespace MigrationTools.Endpoints
{
    public class InMemoryWorkItemEndpoint : WorkItemEndpoint
    {
        private List<WorkItemData> _innerList;
        private InMemoryWorkItemEndpointOptions _Options;
        private List<IEndpointEnricher> _EndpointEnrichers;

        public InMemoryWorkItemEndpoint(EndpointEnricherContainer endpointEnrichers, IServiceProvider services, ITelemetryLogger telemetry, ILogger<WorkItemEndpoint> logger) : base(endpointEnrichers, services, telemetry, logger)
        {
            _innerList = new List<WorkItemData>();
            _EndpointEnrichers = new List<IEndpointEnricher>();
        }

        public override int Count => _innerList.Count;
        public override EndpointDirection Direction => _Options.Direction;
        public override IEnumerable<IWorkItemProcessorSourceEnricher> SourceEnrichers => _EndpointEnrichers.Where(e => e.GetType().IsAssignableFrom(typeof(IWorkItemProcessorSourceEnricher))).Select(e => (IWorkItemProcessorSourceEnricher)e);
        public override IEnumerable<IWorkItemProcessorTargetEnricher> TargetEnrichers => _EndpointEnrichers.Where(e => e.GetType().IsAssignableFrom(typeof(IWorkItemProcessorTargetEnricher))).Select(e => (IWorkItemProcessorTargetEnricher)e);

        public override void Configure(IEndpointOptions options)
        {
            _Options = (InMemoryWorkItemEndpointOptions)options;
        }

        public WorkItemData CreateNewFrom(WorkItemData source)
        {
            _innerList.Add(source);
            return source;
        }

        public override void Filter(IEnumerable<WorkItemData> workItems)
        {
            var ids = (from x in workItems select x.Id);
            _innerList = (from x in _innerList
                          where !ids.Contains(x.Id)
                          select x).ToList();
        }

        public override IEnumerable<WorkItemData> GetWorkItems()
        {
            return _innerList;
        }

        public override IEnumerable<WorkItemData> GetWorkItems(IWorkItemQuery query)
        {
            return GetWorkItems();
        }

        public override void PersistWorkItem(WorkItemData source)
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