using System;
using Microsoft.Extensions.Logging;
using MigrationTools.Processors;

namespace MigrationTools.Enrichers
{
    public class PauseAfterEachItem : WorkItemProcessorEnricher
    {
        private PauseAfterEachItemOptions _Options;

        public PauseAfterEachItemOptions Options
        {
            get { return _Options; }
        }

        public PauseAfterEachItem(IMigrationEngine engine, ILogger<WorkItemProcessorEnricher> logger) : base(engine, logger)
        {
        }

        [Obsolete("Old v1 arch: this is a v2 class", true)]
        public override void Configure(bool save = true, bool filter = true)
        {
            throw new System.NotImplementedException();
        }

        public override void Configure(IProcessorEnricherOptions options)
        {
            _Options = (PauseAfterEachItemOptions)options;
        }

        [Obsolete("Old v1 arch: this is a v2 class", true)]
        public override int Enrich(_EngineV1.DataContracts.WorkItemData sourceWorkItem, _EngineV1.DataContracts.WorkItemData targetWorkItem)
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
    }
}