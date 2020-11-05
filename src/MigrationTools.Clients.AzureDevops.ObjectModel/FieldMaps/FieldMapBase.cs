using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Containers;
using MigrationTools._EngineV1.DataContracts;

namespace MigrationTools.FieldMaps.AzureDevops.ObjectModel
{
    public abstract class FieldMapBase : IFieldMap
    {
        protected IFieldMapConfig _Config;

        public FieldMapBase(ILogger<FieldMapBase> logger)
        {
            Log = logger;
        }

        public virtual void Configure(IFieldMapConfig config)
        {
            _Config = config;
        }

        public void Execute(WorkItemData source, WorkItemData target)
        {
            try
            {
                InternalExecute(source.ToWorkItem(), target.ToWorkItem());
            }
            catch (Exception ex)
            {
                Log.LogError(ex, "Field mapp fault",
                       new Dictionary<string, string> {
                            { "Source", source.ToWorkItem().Id.ToString() },
                            { "Target",  target.ToWorkItem().Id.ToString()}
                       });
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
    }
}