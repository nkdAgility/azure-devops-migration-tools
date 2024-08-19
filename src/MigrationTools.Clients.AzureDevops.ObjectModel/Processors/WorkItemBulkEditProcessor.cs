using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using MigrationTools;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Configuration.Processing;
using MigrationTools.DataContracts;

using Microsoft.Extensions.Options;
using MigrationTools.Tools;
using MigrationTools.Processors.Infrastructure;

namespace MigrationTools.Processors
{
    /// <summary>
    /// This processor allows you to make changes in place where we load from teh Target and update the Target. This is used for bulk updates with the most common reason being a process template change.
    /// </summary>
    /// <processingtarget>WorkItem</processingtarget>
    public class WorkItemBulkEditProcessor : TfsStaticProcessorBase
    {
        private WorkItemBulkEditProcessorOptions _config;

        public WorkItemBulkEditProcessor(IOptions<WorkItemBulkEditProcessorOptions> options, TfsStaticTools tfsStaticEnrichers, StaticTools staticEnrichers, IServiceProvider services, IMigrationEngine me, ITelemetryLogger telemetry, ILogger<TfsStaticProcessorBase> logger) : base(tfsStaticEnrichers, staticEnrichers, services, me, telemetry, logger)
        {
            _config = options.Value;
        }


        public override string Name
        {
            get
            {
                return typeof(WorkItemBulkEditProcessor).Name;
            }
        }

        protected override void InternalExecute()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            //////////////////////////////////////////////////
            List<WorkItemData> workitems = Engine.Target.WorkItems.GetWorkItems(_config.WIQLQuery);
            Log.LogInformation("Update {0} work items?", workitems.Count);
            //////////////////////////////////////////////////
            int current = workitems.Count;
            int count = 0;
            long elapsedms = 0;
            foreach (WorkItemData workitem in workitems)
            {
                Stopwatch witstopwatch = Stopwatch.StartNew();
                workitem.ToWorkItem().Open();
                Log.LogInformation("Processing work item {0} - Type:{1} - ChangedDate:{2} - CreatedDate:{3}", workitem.Id, workitem.Type, workitem.ToWorkItem().ChangedDate.ToShortDateString(), workitem.ToWorkItem().CreatedDate.ToShortDateString());
               StaticEnrichers.FieldMappingTool.ApplyFieldMappings(workitem);

                if (workitem.ToWorkItem().IsDirty)
                {
                    if (!_config.WhatIf)
                    {
                        try
                        {
                            workitem.SaveToAzureDevOps();
                        }
                        catch (Exception)
                        {
                            System.Threading.Thread.Sleep(5000);
                            workitem.SaveToAzureDevOps();
                        }
                    }
                    else
                    {
                        Log.LogWarning("No save done: (What IF: enabled)");
                    }
                }
                else
                {
                    Log.LogWarning("No save done: (IsDirty: false)");
                }

                witstopwatch.Stop();
                elapsedms = elapsedms + witstopwatch.ElapsedMilliseconds;
                current--;
                count++;
                TimeSpan average = new TimeSpan(0, 0, 0, 0, (int)(elapsedms / count));
                TimeSpan remaining = new TimeSpan(0, 0, 0, 0, (int)(average.TotalMilliseconds * current));
                Log.LogWarning("Average time of {0} per work item and {1} estimated to completion", string.Format(@"{0:s\:fff} seconds", average), string.Format(@"{0:%h} hours {0:%m} minutes {0:s\:fff} seconds", remaining));
            }
            //////////////////////////////////////////////////
            stopwatch.Stop();
            Log.LogWarning("DONE in {Elapsed} seconds", stopwatch.Elapsed.ToString("c"));
        }
    }
}