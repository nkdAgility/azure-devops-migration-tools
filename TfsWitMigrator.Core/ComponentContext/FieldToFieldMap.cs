using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Diagnostics;
using Microsoft.ApplicationInsights;

namespace TfsWitMigrator.Core.ComponentContext
{
    public class FieldToFieldMap : IFieldMap
    {
        private string sourceField;
        private string targetField;

        public FieldToFieldMap(string sourceField, string targetField)
        {
            this.sourceField = sourceField;
            this.targetField = targetField;
        }

        public void Execute(WorkItem source, WorkItem target)
        {
            if (source.Fields.Contains(sourceField))
            {
                // to tag
                Trace.WriteLine(string.Format("  [READY] field map {0}:{1} to {2}:{3}", source.Id, sourceField, target.Id, targetField));
                try
                {
                    target.Fields[targetField].Value = source.Fields[sourceField].Value;
                } catch (Exception ex)
                {
                    Trace.WriteLine(string.Format("  [FAIL] field map {0}:{1} to {2}:{3}", source.Id, sourceField, target.Id, targetField));
                    TelemetryClient tc = new TelemetryClient();
                    tc.TrackException(ex);
                }
                Trace.WriteLine(string.Format("  [UPDATE] field mapped {0}:{1} to {2}:{3}", source.Id, sourceField, target.Id, targetField ));
            }
        }
    }
}
