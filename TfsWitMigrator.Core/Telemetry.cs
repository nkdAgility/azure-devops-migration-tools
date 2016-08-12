using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.TraceListener;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSTS.DataBulkEditor.Engine
{
   public static class Telemetry
    {
        private const string applicationInsightsKey = "a7622e0a-0b81-4be1-9e85-81d500642b6f";
        private static TelemetryClient telemetryClient;

        public static TelemetryClient Current { get {
                if (telemetryClient == null)
                {
                    InitiliseTelemetry();
                }
                return telemetryClient;
            } }

        public static void InitiliseTelemetry()
        {
            Trace.Listeners.Add(new ApplicationInsightsTraceListener(applicationInsightsKey));
            Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration.Active.InstrumentationKey = applicationInsightsKey;
            telemetryClient = new TelemetryClient();
            telemetryClient.InstrumentationKey = applicationInsightsKey;
            telemetryClient.Context.User.Id = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            telemetryClient.Context.Session.Id = Guid.NewGuid().ToString();
        }
    }
}
