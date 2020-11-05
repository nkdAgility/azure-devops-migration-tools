using System;
using MigrationTools._Enginev1.DataContracts;

namespace MigrationTools.Enrichers
{
    public interface IWorkItemProcessorEnricher : IProcessorEnricher
    {
        void Configure(bool save = true, bool filter = true);

        [Obsolete("We are migrating to a new model. This is the old one.")]
        int Enrich(WorkItemData sourceWorkItem, WorkItemData targetWorkItem);
    }
}