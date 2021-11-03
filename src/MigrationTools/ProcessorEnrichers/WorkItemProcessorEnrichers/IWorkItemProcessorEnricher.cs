using System;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using MigrationTools._EngineV1.DataContracts;

namespace MigrationTools.Enrichers
{
    public interface IWorkItemProcessorEnricher : IProcessorEnricher
    {
        void Configure(bool save = true, bool filter = true);

        [Obsolete("We are migrating to a new model. This is the old one.")]
        int Enrich(WorkItemData sourceWorkItem, WorkItemData targetWorkItem, WorkItemTrackingHttpClient witClient,
            string project);
    }
}