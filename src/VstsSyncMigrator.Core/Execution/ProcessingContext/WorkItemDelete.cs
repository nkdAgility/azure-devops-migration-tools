using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MigrationTools.Configuration;
using MigrationTools;
using MigrationTools.Clients.AzureDevops.ObjectModel.Clients;
using MigrationTools.Configuration.Processing;
using Serilog;
using MigrationTools.DataContracts;

namespace VstsSyncMigrator.Engine
{
    public class WorkItemDelete : StaticProcessorBase
    {
        WorkItemDeleteConfig _config;

        public WorkItemDelete(IServiceProvider services, IMigrationEngine me, ITelemetryLogger telemetry) : base(services, me, telemetry)
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
                    _config.QueryBit, _config.OrderBit);
            var workItems = Engine.Source.WorkItems.GetWorkItems(sourceQuery);
            Log.Information("We are going to delete {sourceWorkItemsCount} work items?", workItems.Count);

            Console.WriteLine("Enter the number of work Items that we will be deleting! Then hit Enter e.g. 21");
            string result = Console.ReadLine();
            if (int.Parse(result) != workItems.Count)
            {
                Log.Warning("USER ABORTED by selecting a number other than {sourceWorkItemsCount}", workItems.Count);
                return;
            }

            //////////////////////////////////////////////////
            int current = workItems.Count;
            //int count = 0;
            //long elapsedms = 0;
            var tobegone = (from WorkItemData wi in workItems  select int.Parse(wi.Id)).ToList();

            foreach (int begone in tobegone)
            {
                ((WorkItemMigrationClient)Engine.Target.WorkItems).Store.DestroyWorkItems(new List<int>() { begone });
                Trace.WriteLine(string.Format("Deleted {0}", begone));
            }

            
            //////////////////////////////////////////////////
            stopwatch.Stop();
            Console.WriteLine(@"DONE in {0:%h} hours {0:%m} minutes {0:s\:fff} seconds", stopwatch.Elapsed);
        }

    }
}