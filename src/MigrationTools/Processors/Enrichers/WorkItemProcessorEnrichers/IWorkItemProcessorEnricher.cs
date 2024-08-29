using System;
using MigrationTools.DataContracts;

namespace MigrationTools.Enrichers
{
    public interface IWorkItemProcessorEnricher : IProcessorEnricher
    {

        [Obsolete("We are migrating to a new model. This is the old one.")]
        int Enrich(WorkItemData sourceWorkItem, WorkItemData targetWorkItem);
    }
}