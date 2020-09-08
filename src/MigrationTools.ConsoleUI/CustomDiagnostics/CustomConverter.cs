using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;
using Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights.TelemetryConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsMigrationTools.CustomDiagnostics
{
    public class CustomConverter : TraceTelemetryConverter
    {
        public override IEnumerable<ITelemetry> Convert(LogEvent logEvent, IFormatProvider formatProvider)
        {
            // first create a default TraceTelemetry using the sink's default logic
            // .. but without the log level, and (rendered) message (template) included in the Properties
            foreach (ITelemetry telemetry in base.Convert(logEvent, formatProvider))
            {
                // Add Common Stuff
                telemetry.Context.Device.OperatingSystem = Environment.OSVersion.ToString();
                telemetry.Context.Component.Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                telemetry.Context.User.Id = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

                if (logEvent.Properties.ContainsKey("SessionID"))
                {
                    telemetry.Context.Session.Id = logEvent.Properties["SessionID"].ToString();
                }
                // post-process the telemetry's context to contain the operation id
                if (logEvent.Properties.ContainsKey("operation_Id"))
                {
                    telemetry.Context.Operation.Id = logEvent.Properties["operation_Id"].ToString();
                }
                // post-process the telemetry's context to contain the operation parent id
                if (logEvent.Properties.ContainsKey("operation_parentId"))
                {
                    telemetry.Context.Operation.ParentId = logEvent.Properties["operation_parentId"].ToString();
                }
                // typecast to ISupportProperties so you can manipulate the properties as desired
                ISupportProperties propTelematry = (ISupportProperties)telemetry;

                // find redundent properties
                var removeProps = new[] { "UserId", "operation_parentId", "operation_Id", "SessionID" };
                removeProps = removeProps.Where(prop => propTelematry.Properties.ContainsKey(prop)).ToArray();

                foreach (var prop in removeProps)
                {
                    // remove redundent properties
                    propTelematry.Properties.Remove(prop);
                }

                yield return telemetry;
            }
        }
    }
}
