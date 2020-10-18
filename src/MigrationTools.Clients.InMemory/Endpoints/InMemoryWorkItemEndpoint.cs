using System.Linq;
using MigrationTools.Configuration;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;

namespace MigrationTools.Endpoints
{
    public class InMemoryWorkItemEndpoint : WorkItemEndpoint
    {
        public InMemoryWorkItemEndpoint(InMemoryWorkItemEndpointOptions inMemoryWorkItemEndpointOptions) : base(inMemoryWorkItemEndpointOptions)
        {
        }

        public override void PersistWorkItem(WorkItemData source)
        {
            var found = (from x in _innerList where x.Id == source.Id select x).SingleOrDefault();
            if (found is null)
            {
                found = CreateNewFrom(source);
            }
            foreach (IWorkItemTargetEnricher enricher in TargetEnrichers)
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

        public WorkItemData CreateNewFrom(WorkItemData source)
        {
            _innerList.Add(source);
            return source;
        }
    }
}