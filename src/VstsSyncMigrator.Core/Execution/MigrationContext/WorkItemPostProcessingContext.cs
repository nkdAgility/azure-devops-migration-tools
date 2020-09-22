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

namespace VstsSyncMigrator.Engine
{
    public class WorkItemPostProcessingContext : MigrationContextBase
    {

        private WorkItemPostProcessingConfig _config;
        //private IList<string> _workItemTypes;
        //private IList<int> _workItemIDs;
        // private string _queryBit;

        public WorkItemPostProcessingContext(IServiceProvider services, ITelemetryLogger telemetry) : base(services, telemetry)
        {
        }

        public override void Configure(ITfsProcessingConfig config)
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

        internal override void InternalExecute()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
			//////////////////////////////////////////////////
			WorkItemStoreContext sourceStore = new WorkItemStoreContext(me.Source, WorkItemStoreFlags.None, Telemetry);
            TfsQueryContext tfsqc = new TfsQueryContext(sourceStore, Telemetry);
            tfsqc.AddParameter("TeamProject", me.Source.Config.Project);

            //Builds the constraint part of the query
            string constraints = BuildQueryBitConstraints();

            tfsqc.Query = string.Format(@"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject {0} ORDER BY [System.Id] ", constraints);

            WorkItemCollection sourceWIS = tfsqc.Execute();
            Trace.WriteLine(string.Format("Migrate {0} work items?", sourceWIS.Count));
            //////////////////////////////////////////////////
            WorkItemStoreContext targetStore = new WorkItemStoreContext(me.Target, WorkItemStoreFlags.BypassRules, Telemetry);
            Project destProject = targetStore.GetProject();
            Trace.WriteLine(string.Format("Found target project as {0}", destProject.Name));


            int current = sourceWIS.Count;
            int count = 0;
            long elapsedms = 0;
            foreach (WorkItem sourceWI in sourceWIS)
            {
                Stopwatch witstopwatch = Stopwatch.StartNew();
				WorkItem targetFound;
                targetFound = targetStore.FindReflectedWorkItem(sourceWI, false);
                Trace.WriteLine(string.Format("{0} - Updating: {1}-{2}", current, sourceWI.Id, sourceWI.Type.Name));
                if (targetFound == null)
                {
                    Trace.WriteLine(string.Format("{0} - WARNING: does not exist {1}-{2}", current, sourceWI.Id, sourceWI.Type.Name));
                }
                else
                {
                    Console.WriteLine("...Exists");
                    targetFound.Open();
                    me.ApplyFieldMappings(sourceWI, targetFound);
                    if (targetFound.IsDirty)
                    {
                        try
                        {
                            targetFound.Save();
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
                    sourceWI.Close();
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

            if (me.TypeDefinitionMaps.Items != null && me.TypeDefinitionMaps.Items.Count > 0)
            {
                if (me.TypeDefinitionMaps.Items.Count == 1)
                {
                    constraints += string.Format(" AND [System.WorkItemType] = '{0}' ", me.TypeDefinitionMaps.Items.Keys.First());
                }
                else
                {
                    constraints += string.Format(" AND [System.WorkItemType] IN ('{0}') ", string.Join("','", me.TypeDefinitionMaps.Items.Keys));
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