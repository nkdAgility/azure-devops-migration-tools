using System;
using Microsoft.Extensions.Logging;
using MigrationTools._EngineV1.DataContracts;

namespace MigrationTools.Enrichers
{
    public class SkipToFinalRevisedWorkItemType : WorkItemProcessorEnricher
    {
        private SkipToFinalRevisedWorkItemTypeOptions _Options;

        public SkipToFinalRevisedWorkItemTypeOptions Options
        {
            get { return _Options; }
        }

        public SkipToFinalRevisedWorkItemType(IMigrationEngine engine, ILogger<WorkItemProcessorEnricher> logger) : base(engine, logger)
        {
        }

        [Obsolete("Old v1 arch: this is a v2 class", true)]
        public override void Configure(bool save = true, bool filter = true)
        {
            throw new System.NotImplementedException();
        }

        public override void Configure(IProcessorEnricherOptions options)
        {
            _Options = (SkipToFinalRevisedWorkItemTypeOptions)options;
        }

        [Obsolete("Old v1 arch: this is a v2 class", true)]
        public override int Enrich(WorkItemData sourceWorkItem, WorkItemData targetWorkItem)
        {
            throw new System.NotImplementedException();
        }
    }
}