using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.ApplicationInsights;
using System.Diagnostics;

namespace VstsSyncMigrator.Engine.ComponentContext
{
    public abstract class FieldMapBase : IFieldMap
    {

        public void Execute(WorkItem source, WorkItem target)
        {
            try
            {
                InternalExecute(source, target);
            }
            catch (Exception ex)
            {
                Telemetry.Current.TrackException(ex,
                       new Dictionary<string, string> {
                            { "Source", source.Id.ToString() },
                            { "Target",  target.Id.ToString()}
                       });
                Trace.TraceError(string.Format("  [EXCEPTION] {0}", ex.Message));
            }            
        }
        public string Name
        {
            get
            {
                return this.GetType().Name;
            }
        }

        internal abstract void InternalExecute(WorkItem source, WorkItem target);
    }
}
