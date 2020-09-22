using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationTools
{
   public static class Telemetry
    {
        private const string applicationInsightsKey = "4b9bb17b-c7ee-43e5-b220-ec6db2c33373";
        private static TelemetryClient telemetryClient;
        private static bool enableTrace = false;

        public static bool EnableTrace { get { return enableTrace; } set { enableTrace = value; } }

        [Obsolete()]
        public static TelemetryClient Current { get {
                return telemetryClient = GetTelemiteryClient(); 
            } }

        public static TelemetryClient GetTelemiteryClient()
        {
            if (telemetryClient is null)
            {
                var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
                telemetryConfiguration.InstrumentationKey = applicationInsightsKey;
                var tc = new TelemetryClient(telemetryConfiguration);
                tc.Context.Session.Id = Guid.NewGuid().ToString();
                tc.Context.Device.OperatingSystem = Environment.OSVersion.ToString();
                tc.Context.Component.Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                AddModules(telemetryConfiguration);                
                telemetryClient = tc;
            }
            return telemetryClient;
        }

        private static void AddModules(TelemetryConfiguration telemetryConfiguration)
        {
            var perfCollectorModule = new PerformanceCollectorModule();
            perfCollectorModule.Counters.Add(new PerformanceCounterCollectionRequest(
              string.Format(@"\.NET CLR Memory({0})\# GC Handles", System.AppDomain.CurrentDomain.FriendlyName), "GC Handles"));
            perfCollectorModule.Initialize(telemetryConfiguration);
        }
    }
}
