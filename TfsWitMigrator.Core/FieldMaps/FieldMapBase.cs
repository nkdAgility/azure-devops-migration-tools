using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.ApplicationInsights;
using System.Diagnostics;

namespace VSTS.DataBulkEditor.Engine.ComponentContext
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
                TelemetryClient tc = new TelemetryClient();
                tc.TrackException(ex);
                Trace.TraceWarning(string.Format("  [EXCEPTION] {0}", ex.Message));
                throw;
            }            
        }

        internal abstract void InternalExecute(WorkItem source, WorkItem target);
    }
}
