using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using MigrationTools;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.DataContracts;
using MigrationTools._EngineV1.Processors;

namespace VstsSyncMigrator.Engine
{
    public class FakeProcessor : MigrationProcessorBase
    {
        public FakeProcessor(IMigrationEngine engine, IServiceProvider services, ITelemetryLogger telemetry, ILogger<FakeProcessor> logger) : base(engine, services, telemetry, logger)
        {
        }

        public override string Name
        {
            get
            {
                return "FakeProcessor";
            }
        }

        public override void Configure(IProcessorConfig config)
        {
            // FakeProcessorConfig config
        }

        protected override void InternalExecute()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            //////////////////////////////////////////////////

            var query = @"SELECT [System.Id] FROM WorkItems WHERE  [System.TeamProject] = @TeamProject ";// AND [System.Id] = 188708 ";
            List<WorkItemData> sourceWIS = Engine.Source.WorkItems.GetWorkItems(query);
            Log.LogDebug("Migrate {0} work items?", sourceWIS.Count);
            //////////////////////////////////////////////////

            int current = sourceWIS.Count;
            foreach (WorkItemData sourceWI in sourceWIS)
            {
                System.Threading.Thread.Sleep(10);
            }
            stopwatch.Stop();
            Console.WriteLine(@"DONE in {0:%h} hours {0:%m} minutes {0:s\:fff} seconds", stopwatch.Elapsed);
        }
    }
}