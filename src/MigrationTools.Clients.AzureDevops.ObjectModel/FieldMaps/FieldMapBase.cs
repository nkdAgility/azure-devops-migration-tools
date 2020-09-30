using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.ApplicationInsights;
using System.Diagnostics;
using MigrationTools;
using Serilog;
using MigrationTools.Configuration;
using MigrationTools.Engine;
using MigrationTools.DataContracts;
using MigrationTools.Engine.Containers;
using Microsoft.Extensions.Logging;

namespace MigrationTools.Clients.AzureDevops.ObjectModel.FieldMaps
{
    public abstract class FieldMapBase : IFieldMap
    {
        protected IFieldMapConfig _Config;

        public FieldMapBase(ILogger<FieldMapBase> logger)
        {
            Logger = logger;
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
                Log.Error(ex, "Field mapp fault", 
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
        public ILogger<FieldtoFieldMultiMap> Logger { get; }

        internal abstract void InternalExecute(WorkItem source, WorkItem target);


    }
}
