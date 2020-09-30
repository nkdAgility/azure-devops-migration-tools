using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using System.Diagnostics;
using MigrationTools;
using Serilog;
using MigrationTools.Configuration;
using MigrationTools.Engine;
using MigrationTools.DataContracts;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using MigrationTools.Clients.AzureDevops.Rest;
using MigrationTools.Engine.Containers;

namespace MigrationTools.Clients.AzureDevops.Rest.FieldMaps
{
    public abstract class FieldMapBase : IFieldMap
    {
        protected IFieldMapConfig _Config;

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

        internal abstract void InternalExecute(WorkItem source, WorkItem target);


    }
}
