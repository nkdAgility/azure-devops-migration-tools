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
using MigrationTools.Core.Configuration;

namespace VstsSyncMigrator.Engine.ComponentContext
{
    public abstract class FieldMapBase : IFieldMap
    {
        protected IFieldMapConfig _Config;

        public virtual void Configure(IFieldMapConfig config)
        {
            _Config = config;
        }

        public void Execute(WorkItem source, WorkItem target)
        {
            try
            {
                InternalExecute(source, target);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Field mapp fault", 
                       new Dictionary<string, string> {
                            { "Source", source.Id.ToString() },
                            { "Target",  target.Id.ToString()}
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
