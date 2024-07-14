using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Containers;
using MigrationTools.DataContracts;

namespace MigrationTools.FieldMaps.AzureDevops.ObjectModel
{
    public abstract class FieldMapBase : IFieldMap
    {
        protected IFieldMapConfig _Config;
        protected ITelemetryLogger Telemetry;

        public FieldMapBase(ILogger<FieldMapBase> logger, ITelemetryLogger telemetryLogger)
        {
            Log = logger;
            Telemetry = telemetryLogger;
        }

        public virtual void Configure(IFieldMapConfig config)
        {
            _Config = config;
        }

        public void Execute(WorkItemData source, WorkItemData target)
        {
            try
            {
                if (RefactoredToUseWorkItemData)
                {
                    InternalExecute(source, target);
                }
                else
                {
                    InternalExecute(source.ToWorkItem(), target.ToWorkItem());
                }
            }
            catch (Exception ex)
            {
                Log.LogError(ex, "Field map fault",
                       new Dictionary<string, string> {
                            { "Source", source.ToWorkItem().Id.ToString() },
                            { "Target",  target.ToWorkItem().Id.ToString()}
                       });
                Telemetry.TrackException(ex, null, null);
            }
        }

        public string Name
        {
            get
            {
                return this.GetType().Name;
            }
        }

        public abstract string MappingDisplayName { get; }
        public ILogger<FieldMapBase> Log { get; }

        internal abstract void InternalExecute(WorkItem source, WorkItem target);

        internal virtual bool RefactoredToUseWorkItemData => false;

        internal virtual void InternalExecute(WorkItemData source, WorkItemData target)
        {

        }
    }
}