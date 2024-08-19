using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MigrationTools.DataContracts;
using MigrationTools.Processors;
using MigrationTools.Processors.Infrastructure;

namespace MigrationTools.Enrichers
{
    public class PauseAfterEachItem : WorkItemProcessorEnricher
    {
        private PauseAfterEachItemOptions _Options;

        public PauseAfterEachItemOptions Options
        {
            get { return _Options; }
        }

        public IMigrationEngine Engine { get; private set; }

        public PauseAfterEachItem(IServiceProvider services, ILogger<PauseAfterEachItem> logger, ITelemetryLogger telemetryLogger) : base(services, logger, telemetryLogger)
        {
            Engine = Services.GetRequiredService<IMigrationEngine>();
        }

        [Obsolete("Old v1 arch: this is a v2 class", true)]
        public override int Enrich(WorkItemData sourceWorkItem, WorkItemData targetWorkItem)
        {
            throw new System.NotImplementedException();
        }

        public override void ProcessorExecutionAfterProcessWorkItem(IProcessor processor, DataContracts.WorkItemData workitem)
        {
            if (Options.Enabled)
            {
                Console.WriteLine("Do you want to continue? (y/n)");
                if (Console.ReadKey().Key != ConsoleKey.Y)
                {
                    Log.LogCritical("USER ABORTED");
                    Environment.Exit(-1);
                }
            }
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