using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MigrationTools.Core.Configuration.Processing;
using Microsoft.Extensions.Hosting;
using MigrationTools.Core.Configuration;
using MigrationTools;
using MigrationTools.Core.Engine.Processors;

namespace VstsSyncMigrator.Engine
{
    public class FakeProcessor : MigrationProcessorBase
    {
        public override string Name
        {
            get
            {
                return "FakeProcessor";
            }
        }

        public FakeProcessor(IServiceProvider services, ITelemetryLogger telemetry) : base(services, telemetry)
        {

        }


        protected override void InternalExecute()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
			//////////////////////////////////////////////////
			WorkItemStoreContext sourceStore = new WorkItemStoreContext(me.Source, WorkItemStoreFlags.None, Telemetry);
            TfsQueryContext tfsqc = new TfsQueryContext(sourceStore, Telemetry);
            tfsqc.AddParameter("TeamProject", me.Source.Config.Project);
            tfsqc.Query = @"SELECT [System.Id] FROM WorkItems WHERE  [System.TeamProject] = @TeamProject ";// AND [System.Id] = 188708 ";
            WorkItemCollection sourceWIS = tfsqc.Execute();
            Trace.WriteLine(string.Format("Migrate {0} work items?", sourceWIS.Count));
            //////////////////////////////////////////////////
            
            int current = sourceWIS.Count;
            foreach (WorkItem sourceWI in sourceWIS)
            {
                System.Threading.Thread.Sleep(10);
            }
            stopwatch.Stop();
            Console.WriteLine(@"DONE in {0:%h} hours {0:%m} minutes {0:s\:fff} seconds", stopwatch.Elapsed);
        }

        public override void Configure(IProcessorConfig config)
        {
          // FakeProcessorConfig config
        }
    }
}