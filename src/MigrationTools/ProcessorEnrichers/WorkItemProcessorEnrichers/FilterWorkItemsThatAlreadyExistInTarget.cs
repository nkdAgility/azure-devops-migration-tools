using System;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using MigrationTools._EngineV1.DataContracts;

namespace MigrationTools.Enrichers
{
    public class FilterWorkItemsThatAlreadyExistInTarget : WorkItemProcessorEnricher
    {
        private FilterWorkItemsThatAlreadyExistInTargetOptions _Options;

        public FilterWorkItemsThatAlreadyExistInTargetOptions Options
        {
            get { return _Options; }
        }

        public FilterWorkItemsThatAlreadyExistInTarget(IMigrationEngine engine, ILogger<WorkItemProcessorEnricher> logger) : base(engine, logger)
        {
        }

        public override void Configure(IProcessorEnricherOptions options)
        {
            _Options = (FilterWorkItemsThatAlreadyExistInTargetOptions)options;
        }

        [Obsolete("Old v1 arch: this is a v2 class", true)]
        public override int Enrich(WorkItemData sourceWorkItem, WorkItemData targetWorkItem,
            WorkItemTrackingHttpClient witClient, string project)
        {
            throw new System.NotImplementedException();
        }
    }
}