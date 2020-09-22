using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using MigrationTools.Core.Configuration.Processing;
using MigrationTools.Core.Configuration;
using Microsoft.Extensions.Hosting;
using MigrationTools;

namespace VstsSyncMigrator.Engine
{
    public class WorkItemUpdateAreasAsTagsContext : ProcessingContextBase
    {

        WorkItemUpdateAreasAsTagsConfig config;

        public WorkItemUpdateAreasAsTagsContext(IServiceProvider services, MigrationEngine me, ITelemetryLogger telemetry) : base(services, me, telemetry)
        {
        }

        public override void Configure(ITfsProcessingConfig config)
        {
            this.config = (WorkItemUpdateAreasAsTagsConfig)config;
        }

        public override string Name
        {
            get
            {
                return "WorkItemUpdateAreasAsTagsContext";
            }
        }

        internal override void InternalExecute()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
			//////////////////////////////////////////////////
			WorkItemStoreContext targetStore = new WorkItemStoreContext(me.Target, WorkItemStoreFlags.BypassRules, Telemetry);

            TfsQueryContext tfsqc = new TfsQueryContext(targetStore, Telemetry);
            tfsqc.AddParameter("TeamProject", me.Target.Config.Project);
            tfsqc.AddParameter("AreaPath", config.AreaIterationPath);
            tfsqc.Query = @"SELECT [System.Id], [System.Tags] FROM WorkItems WHERE  [System.TeamProject] = @TeamProject and [System.AreaPath] under @AreaPath";
            WorkItemCollection  workitems = tfsqc.Execute();
            Trace.WriteLine(string.Format("Update {0} work items?", workitems.Count));
            //////////////////////////////////////////////////
            int current = workitems.Count;
            int count = 0;
            long elapsedms = 0;
            foreach (WorkItem workitem in workitems)
            {
                Stopwatch witstopwatch = Stopwatch.StartNew();

				Trace.WriteLine(string.Format("{0} - Updating: {1}-{2}", current, workitem.Id, workitem.Type.Name));
                string areaPath = workitem.AreaPath;
                List<string> bits = new List<string>(areaPath.Split(char.Parse(@"\"))).Skip(4).ToList();
                List<string> tags = workitem.Tags.Split(char.Parse(@";")).ToList();
                List<string> newTags = tags.Union(bits).ToList();
                string newTagList = string.Join(";", newTags.ToArray());
                if (newTagList != workitem.Tags)
                { 
                workitem.Open();
                workitem.Tags = newTagList;
                workitem.Save();

            }

            witstopwatch.Stop();
                elapsedms = elapsedms + witstopwatch.ElapsedMilliseconds;
                current--;
                count++;
                TimeSpan average = new TimeSpan(0, 0, 0, 0, (int)(elapsedms / count));
                TimeSpan remaining = new TimeSpan(0, 0, 0, 0, (int)(average.TotalMilliseconds * current));
                Trace.WriteLine(string.Format("Average time of {0} per work item and {1} estimated to completion", string.Format(@"{0:s\:fff} seconds", average), string.Format(@"{0:%h} hours {0:%m} minutes {0:s\:fff} seconds", remaining)));
            }
            //////////////////////////////////////////////////
            stopwatch.Stop();
            Console.WriteLine(@"DONE in {0:%h} hours {0:%m} minutes {0:s\:fff} seconds", stopwatch.Elapsed);
        }

    }
}