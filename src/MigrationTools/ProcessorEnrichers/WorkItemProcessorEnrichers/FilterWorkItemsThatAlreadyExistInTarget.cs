using System;
using Microsoft.Extensions.Logging;
using MigrationTools._Enginev1.DataContracts;

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

        [Obsolete("Old v1 arch: this is a v2 class", true)]
        public override void Configure(bool save = true, bool filter = true)
        {
            throw new System.NotImplementedException();
        }

        public override void Configure(IProcessorEnricherOptions options)
        {
            _Options = (FilterWorkItemsThatAlreadyExistInTargetOptions)options;
        }

        [Obsolete("Old v1 arch: this is a v2 class", true)]
        public override int Enrich(WorkItemData sourceWorkItem, WorkItemData targetWorkItem)
        {
            throw new System.NotImplementedException();
        }
    }
}