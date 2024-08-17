using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MigrationTools;
using MigrationTools._EngineV1.Clients;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Configuration.Processing;
using MigrationTools.DataContracts;
using VstsSyncMigrator._EngineV1.Processors;
using Microsoft.Extensions.Options;
using MigrationTools.Tools;
using MigrationTools.Processors.Infra;

namespace MigrationTools.Processors
{
    /// <summary>
    /// A common issue with older *TFS/Azure DevOps* instances is the proliferation of `Area Paths`. With the use of `Area Path` for `Teams` and the addition of the `Node Name` column option these extensive tag hierarchies should instad be moved to tags.
    /// </summary>
    /// <status>Beta</status>
    /// <processingtarget>Work Item</processingtarget>
    public class WorkItemUpdateAreasAsTagsProcessor : TfsStaticProcessorBase
    {
        private WorkItemUpdateAreasAsTagsProcessorOptions _config;

        public WorkItemUpdateAreasAsTagsProcessor(IOptions<WorkItemUpdateAreasAsTagsProcessorOptions> options, TfsStaticTools tfsStaticEnrichers, StaticTools staticEnrichers, IServiceProvider services, IMigrationEngine me, ITelemetryLogger telemetry, ILogger<TfsStaticProcessorBase> logger) : base(tfsStaticEnrichers, staticEnrichers, services, me, telemetry, logger)
        {
            _config = options.Value;
        }

        public override string Name
        {
            get
            {
                return typeof(WorkItemUpdateAreasAsTagsProcessor).Name;
            }
        }

        protected override void InternalExecute()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            //////////////////////////////////////////////////

            IWorkItemQueryBuilder wiqb = Services.GetRequiredService<IWorkItemQueryBuilder>();
            wiqb.AddParameter("AreaPath", _config.AreaIterationPath);
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