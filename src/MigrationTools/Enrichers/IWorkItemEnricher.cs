using System;
using MigrationTools.DataContracts;

namespace MigrationTools.Enrichers
{
    public interface IWorkItemEnricher : IEndpointEnricher
    {
        void Configure(bool save = true, bool filter = true);

        [Obsolete("We are migrating to a new model. This is the old one.")]
        int Enrich(WorkItemData sourceWorkItem, WorkItemData targetWorkItem);
    }
}