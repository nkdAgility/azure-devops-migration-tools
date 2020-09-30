using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MigrationTools.Configuration.Processing;
using Microsoft.Extensions.Hosting;
using MigrationTools.Configuration;
using MigrationTools;
using MigrationTools.Clients.AzureDevops.ObjectModel;
using MigrationTools.Engine.Processors;
using MigrationTools;
using MigrationTools.Clients;
using Microsoft.Extensions.DependencyInjection;
using MigrationTools.DataContracts;

namespace VstsSyncMigrator.Engine
{
    public class WorkItemPostProcessingContext : MigrationProcessorBase
    {

        private WorkItemPostProcessingConfig _config;
        //private IList<string> _workItemTypes;
        //private IList<int> _workItemIDs;
        // private string _queryBit;

        public WorkItemPostProcessingContext(IMigrationEngine me, IServiceProvider services, ITelemetryLogger telemetry) : base(me, services, telemetry)
        {
        }

        public override void Configure(IProcessorConfig config)
        {
            _config = (WorkItemPostProcessingConfig)config;
        }

        public override string Name
        {
            get
            {
                return "WorkItemPostProcessingContext";
            }
        }
        //public WorkItemPostProcessingContext(MigrationEngine me, WorkItemPostProcessingConfig config, IList<string> wiTypes) : this(me, config)
        //{
        //    _workItemTypes = wiTypes;
        //}

        //public WorkItemPostProcessingContext(MigrationEngine me, WorkItemPostProcessingConfig config, IList<int> wiIDs) : this(me, config)
        //{
        //    _workItemIDs = wiIDs;
        //}

        //public WorkItemPostProcessingContext(MigrationEngine me, WorkItemPostProcessingConfig config, string queryBit) : this (me, config)
        //{
        //    _queryBit = queryBit;
        //}

        protected override void InternalExecute()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            //////////////////////////////////////////////////
            IWorkItemQueryBuilder wiqb = Services.GetRequiredService<IWorkItemQueryBuilder>();
            //Builds the constraint part of the query
            string constraints = BuildQueryBitConstraints();
            wiqb.Query = string.Format(@"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject {0} ORDER BY [System.Id] ", constraints);

            List<WorkItemData> sourceWIS = Engine.Target.WorkItems.GetWorkItems(wiqb);
            Trace.WriteLine(string.Format("Migrate {0} work items?", sourceWIS.Count));
            //////////////////////////////////////////////////
            ProjectData destProject = Engine.Target.WorkItems.GetProject();
            Trace.WriteLine(string.Format("Found target project as {0}", destProject.Name));


            int current = sourceWIS.Count;
            int count = 0;
            long elapsedms = 0;
            foreach (WorkItemData sourceWI in sourceWIS)
            {
                Stopwatch witstopwatch = Stopwatch.StartNew();
				WorkItemData targetFound;
                targetFound = Engine.Target.WorkItems.FindReflectedWorkItem(sourceWI, false);
                Trace.WriteLine(string.Format("{0} - Updating: {1}-{2}", current, sourceWI.Id, sourceWI.Type));
                if (targetFound == null)
                {
                    Trace.WriteLine(string.Format("{0} - WARNING: does not exist {1}-{2}", current, sourceWI.Id, sourceWI.Type));
                }
                else
                {
                    Console.WriteLine("...Exists");
                    targetFound.ToWorkItem().Open();
                    Engine.FieldMaps.ApplyFieldMappings(sourceWI, targetFound);
                    if (targetFound.ToWorkItem().IsDirty)
                    {
                        try
                        {
                            targetFound.ToWorkItem().Save();
                            Trace.WriteLine(string.Format("          Updated"));
                        }
                        catch (ValidationException ve)
                        {

                            Trace.WriteLine(string.Format("          [FAILED] {0}", ve.ToString()));
                        }

                    }
                    else
                    {
                        Trace.WriteLine(string.Format("          No changes"));
                    }
                    sourceWI.ToWorkItem().Close();
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

        private string BuildQueryBitConstraints()
        {
            string constraints = "";

            if (_config.WorkItemIDs != null && _config.WorkItemIDs.Count > 0)
            {
                if (_config.WorkItemIDs.Count == 1)
                {
                    constraints += string.Format(" AND [System.Id] = {0} ", _config.WorkItemIDs[0]);
                }
                else
                {
                    constraints += string.Format(" AND [System.Id] IN ({0}) ", string.Join(",", _config.WorkItemIDs));
                }
            }

            if (Engine.TypeDefinitionMaps.Items != null && Engine.TypeDefinitionMaps.Items.Count > 0)
            {
                if (Engine.TypeDefinitionMaps.Items.Count == 1)
                {
                    constraints += string.Format(" AND [System.WorkItemType] = '{0}' ", Engine.TypeDefinitionMaps.Items.Keys.First());
                }
                else
                {
                    constraints += string.Format(" AND [System.WorkItemType] IN ('{0}') ", string.Join("','", Engine.TypeDefinitionMaps.Items.Keys));
                }
            }


            if (!String.IsNullOrEmpty(_config.QueryBit))
            {
                constraints += _config.QueryBit;
            }
            return constraints;
        }
    }
}