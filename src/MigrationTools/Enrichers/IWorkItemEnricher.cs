using System;
using MigrationTools.DataContracts;

namespace MigrationTools.Enrichers
{
    public interface IWorkItemEnricher : IEnricher
    {
        void Configure(bool save = true, bool filter = true);

        [Obsolete("We are migrating to a new model. This is the old one.")]
        int Enrich(WorkItemData sourceWorkItem, WorkItemData targetWorkItem);
    }

    public interface IWorkItemSourceEnricher : IWorkItemEnricher
    {
        int EnrichToWorkItem(WorkItemData workItem);
    }

    public interface IWorkItemTargetEnricher : IWorkItemEnricher
    {
        int PersistFromWorkItem(WorkItemData workItem);
    }
}