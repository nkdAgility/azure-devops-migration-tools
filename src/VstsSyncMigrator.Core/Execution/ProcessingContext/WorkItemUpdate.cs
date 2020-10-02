using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using MigrationTools.Configuration.Processing;
using MigrationTools.Configuration;
using Microsoft.Extensions.Hosting;
using MigrationTools;
using MigrationTools.Clients.AzureDevops.ObjectModel;
using MigrationTools;
using MigrationTools.DataContracts;

namespace VstsSyncMigrator.Engine
{
    public class WorkItemUpdate : StaticProcessorBase
    {
        WorkItemUpdateConfig _config;

        public WorkItemUpdate(IServiceProvider services, IMigrationEngine me, ITelemetryLogger telemetry) : base(services, me, telemetry)
        {
            
        }

        public override void Configure(IProcessorConfig config)
        {
            _config = (WorkItemUpdateConfig) config;
        }

        public override string Name
        {
            get
            {
                return "WorkItemUpdate";
            }
        }

        protected override void InternalExecute()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
			//////////////////////////////////////////////////
            var Query = string.Format(@"SELECT [System.Id], [System.Tags] FROM WorkItems WHERE [System.TeamProject] = @TeamProject {0} ORDER BY [System.ChangedDate] desc", _config.WIQLQueryBit);
            List<WorkItemData> workitems = Engine.Target.WorkItems.GetWorkItems(Query);
            Trace.WriteLine(string.Format("Update {0} work items?", workitems.Count));
            //////////////////////////////////////////////////
            int current = workitems.Count;
            int count = 0;
            long elapsedms = 0;
            foreach (WorkItemData workitem in workitems)
            {
                Stopwatch witstopwatch = Stopwatch.StartNew();
				workitem.ToWorkItem().Open();
                Trace.WriteLine(string.Format("Processing work item {0} - Type:{1} - ChangedDate:{2} - CreatedDate:{3}", workitem.Id, workitem.Type, workitem.ToWorkItem().ChangedDate.ToShortDateString(), workitem.ToWorkItem().CreatedDate.ToShortDateString()));
                Engine.FieldMaps.ApplyFieldMappings(workitem);

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
                       
                    } else
                    {
                        Trace.WriteLine("No save done: (What IF: enabled)");
                    }
                    
                } else
                {
                    Trace.WriteLine("No save done: (IsDirty: false)");
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