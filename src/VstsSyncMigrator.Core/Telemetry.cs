using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector;
using Microsoft.ApplicationInsights.TraceListener;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsSyncMigrator.Engine
{
   public static class Telemetry
    {
        private const string applicationInsightsKey = "4b9bb17b-c7ee-43e5-b220-ec6db2c33373";
        private static TelemetryClient telemetryClient;
        private static bool enableTrace = false;

        public static bool EnableTrace { get { return enableTrace; } set { enableTrace = value; } }

        public static TelemetryClient Current { get {
                if (telemetryClient == null)
                {
                    InitiliseTelemetry();
                }
                return telemetryClient;
                // No change
            } }

        public static void InitiliseTelemetry()
        {
            if (enableTrace) { Trace.Listeners.Add(new ApplicationInsightsTraceListener(applicationInsightsKey)); }
            TelemetryConfiguration.Active.InstrumentationKey = applicationInsightsKey;
            telemetryClient = new TelemetryClient();
            telemetryClient.InstrumentationKey = applicationInsightsKey;
            telemetryClient.Context.User.Id = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            telemetryClient.Context.Session.Id = Guid.NewGuid().ToString();
            telemetryClient.Context.Device.OperatingSystem = Environment.OSVersion.ToString();
            telemetryClient.Context.Component.Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Trace.WriteLine(string.Format("SessionID: {0}", telemetryClient.Context.Session.Id));
            AddModules();
            telemetryClient.TrackEvent("ApplicationStarted");
        }

        public static void AddModules()
        {
            var perfCollectorModule = new PerformanceCollectorModule();
            perfCollectorModule.Counters.Add(new PerformanceCounterCollectionRequest(
              string.Format(@"\.NET CLR Memory({0})\# GC Handles", System.AppDomain.CurrentDomain.FriendlyName), "GC Handles"));
            perfCollectorModule.Initialize(TelemetryConfiguration.Active);
        }
    }
}
