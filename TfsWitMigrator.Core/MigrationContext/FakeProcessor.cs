using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace _VSTS.DataBulkEditor.Engine
{
    public class FakeProcessor : MigrationContextBase
    {
        public override string Name
        {
            get
            {
                return "FakeProcessor";
            }
        }

        public FakeProcessor(MigrationEngine me) : base(me)
        {

        }


        internal override void InternalExecute()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //////////////////////////////////////////////////
            WorkItemStoreContext sourceStore = new WorkItemStoreContext(me.Source, WorkItemStoreFlags.None);
            TfsQueryContext tfsqc = new TfsQueryContext(sourceStore);
            tfsqc.AddParameter("TeamProject", me.Source.Name);
            tfsqc.Query = @"SELECT [System.Id] FROM WorkItems WHERE  [System.TeamProject] = @TeamProject ";// AND [System.Id] = 188708 ";
            WorkItemCollection sourceWIS = tfsqc.Execute();
            Trace.WriteLine(string.Format("Migrate {0} work items?", sourceWIS.Count));
            //////////////////////////////////////////////////
            
            int current = sourceWIS.Count;
            int count = 0;
            long elapsedms = 0;
            foreach (WorkItem sourceWI in sourceWIS)
            {
                System.Threading.Thread.Sleep(10);
            }
            stopwatch.Stop();
            Console.WriteLine(@"DONE in {0:%h} hours {0:%m} minutes {0:s\:fff} seconds", stopwatch.Elapsed);
        }

    }
}