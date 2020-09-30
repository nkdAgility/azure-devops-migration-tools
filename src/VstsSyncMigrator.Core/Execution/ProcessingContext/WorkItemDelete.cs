using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using MigrationTools.Configuration;
using Microsoft.Extensions.Hosting;
using MigrationTools;
using MigrationTools;
using MigrationTools.DataContracts;
using MigrationTools.Clients.AzureDevops.ObjectModel.Clients;

namespace VstsSyncMigrator.Engine
{
    public class WorkItemDelete : StaticProcessorBase
    {


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
         
        }

        protected override void InternalExecute()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
			//////////////////////////////////////////////////
            var Query = string.Format(@"SELECT [System.Id] FROM WorkItems WHERE  [System.TeamProject] = @TeamProject  AND [System.AreaPath] UNDER '{0}\_DeleteMe'", Engine.Target.Config.Project);
            List<WorkItemData> workitems = Engine.Target.WorkItems.GetWorkItems(Query);
            Trace.WriteLine(string.Format("Update {0} work items?", workitems.Count));
            //////////////////////////////////////////////////
            int current = workitems.Count;
            //int count = 0;
            //long elapsedms = 0;
            var tobegone = (from WorkItem wi in workitems where wi.AreaPath.Contains("_DeleteMe")  select wi.Id).ToList();

            foreach (int begone in tobegone)
            {
                ((WorkItemMigrationClient)Engine.Target).Store.DestroyWorkItems(new List<int>() { begone });
                Trace.WriteLine(string.Format("Deleted {0}", begone));
            }

            
            //////////////////////////////////////////////////
            stopwatch.Stop();
            Console.WriteLine(@"DONE in {0:%h} hours {0:%m} minutes {0:s\:fff} seconds", stopwatch.Elapsed);
        }

    }
}