using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools;
using MigrationTools._EngineV1.Processors;
using MigrationTools.Clients;
using MigrationTools.Configuration;
using MigrationTools.Configuration.Processing;
using MigrationTools.DataContracts;

namespace VstsSyncMigrator.Engine
{
    public class WorkItemPostProcessingContext : MigrationProcessorBase
    {
        private WorkItemPostProcessingConfig _config;

        public WorkItemPostProcessingContext(IMigrationEngine engine, IServiceProvider services, ITelemetryLogger telemetry, ILogger<WorkItemPostProcessingContext> logger) : base(engine, services, telemetry, logger)
        {
        }

        public override string Name
        {
            get
            {
                return "WorkItemPostProcessingContext";
            }
        }

        public override void Configure(IProcessorConfig config)
        {
            _config = (WorkItemPostProcessingConfig)config;
        }

        protected override void InternalExecute()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            //////////////////////////////////////////////////
            IWorkItemQueryBuilder wiqb = Services.GetRequiredService<IWorkItemQueryBuilder>();
            //Builds the constraint part of the query
            string constraints = BuildQueryBitConstraints();
            wiqb.Query = string.Format(@"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject {0} ORDER BY [System.Id] ", constraints);

            List<MigrationTools._EngineV1.DataContracts.WorkItemData> sourceWIS = Engine.Target.WorkItems.GetWorkItems((IWorkItemQueryBuilder)wiqb);
            Log.LogInformation("Migrate {0} work items?", sourceWIS.Count);
            //////////////////////////////////////////////////
            ProjectData destProject = Engine.Target.WorkItems.GetProject();
            Log.LogInformation("Found target project as {0}", destProject.Name);

            int current = sourceWIS.Count;
            int count = 0;
            long elapsedms = 0;
            foreach (MigrationTools._EngineV1.DataContracts.WorkItemData sourceWI in sourceWIS)
            {
                Stopwatch witstopwatch = Stopwatch.StartNew();
                MigrationTools._EngineV1.DataContracts.WorkItemData targetFound;
                targetFound = Engine.Target.WorkItems.FindReflectedWorkItem((MigrationTools._EngineV1.DataContracts.WorkItemData)sourceWI, (bool)false);
                Log.LogInformation("{0} - Updating: {1}-{2}", current, sourceWI.Id, sourceWI.Type);
                if (targetFound == null)
                {
                    Log.LogWarning("{0} - WARNING: does not exist {1}-{2}", current, sourceWI.Id, sourceWI.Type);
                }
                else
                {
                    Log.LogInformation("...Exists");
                    TfsExtensions.ToWorkItem(targetFound).Open();
                    Engine.FieldMaps.ApplyFieldMappings(sourceWI, targetFound);
                    if (TfsExtensions.ToWorkItem(targetFound).IsDirty)
                    {
                        try
                        {
                            TfsExtensions.SaveToAzureDevOps(targetFound);
                            Log.LogInformation("          Updated");
                        }
                        catch (ValidationException ve)
                        {
                            Log.LogError(ve, "          [FAILED] {0}", ve.ToString());
                        }
                    }
                    else
                    {
                        Log.LogInformation("          No changes");
                    }
                    TfsExtensions.ToWorkItem(sourceWI).Close();
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
            Log.LogInformation("DONE in {Elapsed}", stopwatch.Elapsed.ToString("c"));
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

            if (!String.IsNullOrEmpty(_config.WIQLQueryBit))
            {
                constraints += _config.WIQLQueryBit;
            }
            return constraints;
        }
    }
}