using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using MigrationTools;
using MigrationTools.Clients.AzureDevops.ObjectModel.Clients;
using MigrationTools.Configuration;
using MigrationTools.Configuration.Processing;
using MigrationTools.DataContracts;

namespace VstsSyncMigrator.Engine
{
    public class WorkItemDelete : StaticProcessorBase
    {
        private WorkItemDeleteConfig _config;

        public WorkItemDelete(IServiceProvider services, IMigrationEngine me, ITelemetryLogger telemetry, ILogger<WorkItemUpdate> logger) : base(services, me, telemetry, logger)
        {
        }

        public override string Name
        {
            get
            {
                return "WorkItemDelete";
            }
        }

        public override void Configure(IProcessorConfig config)
        {
            _config = (WorkItemDeleteConfig)config;
        }

        protected override void InternalExecute()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            //////////////////////////////////////////////////
            string sourceQuery =
                string.Format(
                    @"SELECT [System.Id], [System.Tags] FROM WorkItems WHERE [System.TeamProject] = @TeamProject {0} ORDER BY {1}",
                    _config.WIQLQueryBit, _config.WIQLOrderBit);
            var workItems = Engine.Target.WorkItems.GetWorkItems(sourceQuery);

            if (workItems.Count > 0)
            {
                Log.LogInformation("We are going to delete {sourceWorkItemsCount} work items?", workItems.Count);

                Console.WriteLine("Enter the number of work Items that we will be deleting! Then hit Enter e.g. 21");
                string result = Console.ReadLine();
                if (int.Parse(result) != workItems.Count)
                {
                    Log.LogWarning("USER ABORTED by selecting a number other than {sourceWorkItemsCount}", workItems.Count);
                    return;
                }

                //////////////////////////////////////////////////
                int current = workItems.Count;

                //int count = 0;
                //long elapsedms = 0;
                var tobegone = (from WorkItemData wi in workItems select int.Parse(wi.Id)).ToList();

                foreach (int begone in tobegone)
                {
                    ((WorkItemMigrationClient)Engine.Target.WorkItems).Store.DestroyWorkItems(new List<int>() { begone });
                    Log.LogInformation("Deleted {0}", begone);
                }
            }
            else
            {
                Log.LogInformation("Nothing to delete");
            }

            //////////////////////////////////////////////////
            stopwatch.Stop();
            Console.WriteLine(@"DONE in {0:%h} hours {0:%m} minutes {0:s\:fff} seconds", stopwatch.Elapsed);
        }
    }
}