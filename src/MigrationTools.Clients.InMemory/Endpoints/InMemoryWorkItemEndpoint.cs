using System.Linq;
using Microsoft.Extensions.Options;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;

namespace MigrationTools.Endpoints
{
    public class InMemoryWorkItemEndpoint : WorkItemEndpoint
    {
        public InMemoryWorkItemEndpoint(IOptions<InMemoryWorkItemEndpointOptions> inMemoryWorkItemEndpointOptions) : base(inMemoryWorkItemEndpointOptions)
        {
        }

        public override void PersistWorkItem(WorkItemData2 source)
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

        private void UpdateWorkItemFrom(WorkItemData2 source, WorkItemData2 target)
        {
            _innerList.Remove(source);
            _innerList.Add(target);
        }

        public WorkItemData2 CreateNewFrom(WorkItemData2 source)
        {
            _innerList.Add(source);
            return source;
        }
    }
}