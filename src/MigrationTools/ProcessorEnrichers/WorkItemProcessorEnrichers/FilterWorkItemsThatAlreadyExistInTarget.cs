using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MigrationTools.DataContracts;
using MigrationTools.Processors;

namespace MigrationTools.Enrichers
{
    public class FilterWorkItemsThatAlreadyExistInTarget : WorkItemProcessorEnricher
    {
        private FilterWorkItemsThatAlreadyExistInTargetOptions _Options;

        public FilterWorkItemsThatAlreadyExistInTargetOptions Options
        {
            get { return _Options; }
        }

        public IMigrationEngine Engine { get; private set; }

        public FilterWorkItemsThatAlreadyExistInTarget(IServiceProvider services, ILogger<FilterWorkItemsThatAlreadyExistInTarget> logger) : base(services, logger)
        {
            Engine = Services.GetRequiredService<IMigrationEngine>();
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

        protected override void RefreshForProcessorType(IProcessor processor)
        {
            throw new NotImplementedException();
        }

        protected override void EntryForProcessorType(IProcessor processor)
        {
            throw new NotImplementedException();
        }
    }
}