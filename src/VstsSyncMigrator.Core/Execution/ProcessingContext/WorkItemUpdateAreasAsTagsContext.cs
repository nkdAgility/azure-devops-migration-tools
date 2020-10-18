using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MigrationTools;
using MigrationTools.Clients;
using MigrationTools.Configuration;
using MigrationTools.Configuration.Processing;
using MigrationTools.DataContracts;

namespace VstsSyncMigrator.Engine
{
    public class WorkItemUpdateAreasAsTagsContext : StaticProcessorBase
    {
        private WorkItemUpdateAreasAsTagsConfig config;

        public WorkItemUpdateAreasAsTagsContext(IServiceProvider services, IMigrationEngine me, ITelemetryLogger telemetry, ILogger<WorkItemUpdateAreasAsTagsContext> logger) : base(services, me, telemetry, logger)
        {
        }

        public override string Name
        {
            get
            {
                return "WorkItemUpdateAreasAsTagsContext";
            }
        }

        public override void Configure(IProcessorConfig config)
        {
            this.config = (WorkItemUpdateAreasAsTagsConfig)config;
        }

        protected override void InternalExecute()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            //////////////////////////////////////////////////

            IWorkItemQueryBuilder wiqb = Services.GetRequiredService<IWorkItemQueryBuilder>();
            wiqb.AddParameter("AreaPath", config.AreaIterationPath);
            wiqb.Query = @"SELECT [System.Id], [System.Tags] FROM WorkItems WHERE  [System.TeamProject] = @TeamProject and [System.AreaPath] under @AreaPath";
            List<WorkItemData> workitems = Engine.Target.WorkItems.GetWorkItems(wiqb);
            Log.LogInformation("Update {0} work items?", workitems.Count);
            //////////////////////////////////////////////////
            int current = workitems.Count;
            int count = 0;
            long elapsedms = 0;
            foreach (WorkItemData workitem in workitems)
            {
                Stopwatch witstopwatch = Stopwatch.StartNew();

                Log.LogInformation("{0} - Updating: {1}-{2}", current, workitem.Id, workitem.Type);
                string areaPath = workitem.ToWorkItem().AreaPath;
                List<string> bits = new List<string>(areaPath.Split(char.Parse(@"\"))).Skip(4).ToList();
                List<string> tags = workitem.ToWorkItem().Tags.Split(char.Parse(@";")).ToList();
                List<string> newTags = tags.Union(bits).ToList();
                string newTagList = string.Join(";", newTags.ToArray());
                if (newTagList != workitem.ToWorkItem().Tags)
                {
                    workitem.ToWorkItem().Open();
                    workitem.ToWorkItem().Tags = newTagList;
                    workitem.SaveToAzureDevOps();
                }

                witstopwatch.Stop();
                elapsedms = elapsedms + witstopwatch.ElapsedMilliseconds;
                current--;
                count++;
                TimeSpan average = new TimeSpan(0, 0, 0, 0, (int)(elapsedms / count));
                TimeSpan remaining = new TimeSpan(0, 0, 0, 0, (int)(average.TotalMilliseconds * current));
                Log.LogInformation("Average time of {0} per work item and {1} estimated to completion", string.Format(@"{0:s\:fff} seconds", average), string.Format(@"{0:%h} hours {0:%m} minutes {0:s\:fff} seconds", remaining));
            }
            //////////////////////////////////////////////////
            stopwatch.Stop();
            Log.LogInformation("DONE in {Elapsed} seconds", stopwatch.Elapsed.ToString("c"));
        }
    }
}