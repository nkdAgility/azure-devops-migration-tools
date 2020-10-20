using System;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;

namespace MigrationTools.Clients.FileSystem.Enrichers
{
    internal class AttachmentWorkItemEnricher : IWorkItemSourceEnricher, IWorkItemTargetEnricher
    {
        public void Configure(bool save = true, bool filter = true)
        {
            throw new NotImplementedException();
        }

        public int Enrich(WorkItemData sourceWorkItem, WorkItemData targetWorkItem)
        {
            throw new NotImplementedException();
        }

        public int EnrichToWorkItem(WorkItemData workItem)
        {
            throw new NotImplementedException();
        }

        public int PersistFromWorkItem(WorkItemData workItem)
        {
            throw new NotImplementedException();
        }
    }
}