using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using MigrationTools;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.DataContracts;
using MigrationTools.Tools;
using Microsoft.Extensions.Options;
using MigrationTools.Enrichers;
using MigrationTools._EngineV1.Clients;

namespace MigrationTools.Processors
{
    /// <summary>
    /// Note: this is only for internal usage. Don't use this in your configurations.
    /// </summary>
    public class FakeProcessor : TfsProcessor
    {
        public FakeProcessor(IOptions<EmptyProcessorOptions> options, TfsCommonTools tfsCommonTools, ProcessorEnricherContainer processorEnrichers, IServiceProvider services, ITelemetryLogger telemetry, ILogger<Processor> logger) : base(options, tfsCommonTools, processorEnrichers, services, telemetry, logger)
        {
        }


        new TfsTeamProjectEndpoint Source => (TfsTeamProjectEndpoint)base.Source;

        new TfsTeamProjectEndpoint Target => (TfsTeamProjectEndpoint)base.Target;

        protected override void InternalExecute()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            //////////////////////////////////////////////////

            var query = @"SELECT [System.Id] FROM WorkItems WHERE  [System.TeamProject] = @TeamProject ";// AND [System.Id] = 188708 ";
            List<WorkItemData> sourceWIS = Source.WorkItems.GetWorkItems(query);
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